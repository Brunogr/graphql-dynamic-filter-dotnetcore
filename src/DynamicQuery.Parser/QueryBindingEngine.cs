using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicQuery.Parser
{
    public static class QueryBindingEngine
    {
        public static DynamicQueryModel<T> Bind<T>(IReadOnlyDictionary<string, string?> queryParams)
        {
            var model = new DynamicQueryModel<T>();
            var itemType = typeof(T);
            var parameter = Expression.Parameter(itemType, "x");

            ApplyFilters(model, queryParams, parameter, itemType);
            ApplyOrder(model, queryParams, parameter);
            ApplyPagination(model, queryParams);
            ApplySelect(model, queryParams, parameter, itemType);

            return model;
        }

        public static DynamicQueryModel<T> Bind<T>(Func<string, string?> valueProvider)
        {
            var queryParams = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in new[] { "filter", "query", "order", "page", "pagesize", "select" })
            {
                var value = valueProvider(key);
                if (!string.IsNullOrWhiteSpace(value))
                    queryParams[key] = value;
            }

            return Bind<T>(queryParams);
        }

        private static void ApplyPagination<T>(DynamicQueryModel<T> model, IReadOnlyDictionary<string, string?> queryParams)
        {
            if (TryGetValue(queryParams, "page", out var page) && int.TryParse(page, out var pageNumber))
                model.Page = pageNumber;

            if (TryGetValue(queryParams, "pagesize", out var pageSize) && int.TryParse(pageSize, out var pageSizeNumber))
                model.PageSize = pageSizeNumber;
        }

        private static void ApplySelect<T>(DynamicQueryModel<T> model, IReadOnlyDictionary<string, string?> queryParams, ParameterExpression parameter, Type itemType)
        {
            if (!TryGetValue(queryParams, "select", out var select))
                return;

            model.SelectText = select;
            var selectFields = select.Split(',');

            var xNew = Expression.New(itemType);
            var bindings = selectFields
                .Select(field => field.Trim())
                .Select(field =>
                {
                    var property = itemType.GetProperty(
                        field,
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (property == null)
                        throw new Exceptions.PropertyNotFoundException(field, itemType.Name);

                    var original = Expression.PropertyOrField(parameter, field);
                    return Expression.Bind(property, original);
                });

            var memberInit = Expression.MemberInit(xNew, bindings);
            model.Select = Expression.Lambda<Func<T, T>>(memberInit, parameter);
        }

        private static void ApplyOrder<T>(DynamicQueryModel<T> model, IReadOnlyDictionary<string, string?> queryParams, ParameterExpression parameter)
        {
            if (!TryGetValue(queryParams, "order", out var order))
                return;

            var orderItems = order.Split('=');
            if (orderItems.Length > 1)
            {
                model.OrderType = (OrderType)Enum.Parse(typeof(OrderType), orderItems[1], ignoreCase: true);
                order = orderItems[0];
            }
            else
            {
                model.OrderType = OrderType.Desc;
            }

            var property = Expression.PropertyOrField(parameter, order);
            var orderExpression = Expression.Lambda<Func<T, object>>(
                Expression.Convert(property, typeof(object)).Reduce(),
                parameter);

            model.Order = orderExpression;
        }

        private static void ApplyFilters<T>(DynamicQueryModel<T> model, IReadOnlyDictionary<string, string?> queryParams, ParameterExpression parameter, Type itemType)
        {
            if (!TryGetValue(queryParams, "filter", out var filter))
                TryGetValue(queryParams, "query", out filter);

            if (string.IsNullOrWhiteSpace(filter))
                return;

            var filterAndValues = filter.Split(',');
            Expression? currentExpression = null;

            foreach (var filterAndValue in filterAndValues)
            {
                if (filterAndValue.Contains('|'))
                {
                    var orExpression = new ExpressionParser();
                    var filterParts = orExpression.DefineOperation(filterAndValue, itemType);
                    var options = filterParts[1].Split('|');

                    for (var index = 0; index < options.Length; index++)
                    {
                        var expression = BuildFilterExpression(
                            parameter,
                            itemType,
                            $"{filterParts[0]}{orExpression.GetOperation()}{options[index]}");

                        if (index == 0)
                        {
                            currentExpression = currentExpression == null
                                ? expression
                                : Expression.AndAlso(currentExpression, expression);
                        }
                        else
                        {
                            currentExpression = Expression.OrElse(currentExpression!, expression);
                        }
                    }
                }
                else
                {
                    var expression = BuildFilterExpression(parameter, itemType, filterAndValue);
                    currentExpression = currentExpression == null
                        ? expression
                        : Expression.AndAlso(currentExpression, expression);
                }
            }

            if (currentExpression != null)
                model.Filter = Expression.Lambda<Func<T, bool>>(currentExpression, parameter);
        }

        private static Expression BuildFilterExpression(ParameterExpression parameter, Type itemType, string filterAndValue)
        {
            var parser = new ExpressionParser(filterAndValue, itemType);
            return parser.GetExpression(parameter);
        }

        private static bool TryGetValue(IReadOnlyDictionary<string, string?> queryParams, string key, out string? value)
        {
            if (queryParams.TryGetValue(key, out value))
                return !string.IsNullOrWhiteSpace(value);

            foreach (var pair in queryParams)
            {
                if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(pair.Value))
                {
                    value = pair.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}
