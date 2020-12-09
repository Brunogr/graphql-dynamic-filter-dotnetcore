using Graphql.NetFramework.DynamicFiltering;
using Graphql.Parser.DynamicFiltering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Graphql.NetFramework.DynamicFiltering
{
    public class DynamicFilterBinder : IModelBinder
    {
        public DynamicFilterBinder()
        {
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var model = Activator.CreateInstance(bindingContext.ModelType);

            var itemType = bindingContext.ModelType.GenericTypeArguments[0];

            var parameter = Expression.Parameter(itemType, "x");

            ExtractFilters(model, bindingContext, parameter, itemType);

            ExtractOrder(model, bindingContext, parameter);

            ExtractPagination(model, bindingContext);

            //bindingContext.Result = ModelBindingResult.Success(model);

            return Task.CompletedTask;
        }


        private static void ExtractSelect(object model, ModelBindingContext bindingContext, ParameterExpression parameter, Type itemType)
        {
            var select = bindingContext.ValueProvider.GetValue("select") != null ? bindingContext.ValueProvider.GetValue("select").AttemptedValue : null;

            if (!string.IsNullOrWhiteSpace(select))
            {
                model.GetType().GetProperty("SelectText").SetValue(model, select);
                var selectFields = select.Split(',');

                // new statement "new Data()"
                var xNew = Expression.New(itemType);

                // create initializers
                var bindings = selectFields.Select(o => o.Trim())
                    .Select(o =>
                    {

                        // property "Field1"
                        var mi = itemType.GetProperty(o, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance);

                        // original value "o.Field1"
                        var xOriginal = Expression.PropertyOrField(parameter, o);

                        // set value "Field1 = o.Field1"
                        return Expression.Bind(mi, xOriginal);
                    }
                );

                // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
                var xInit = Expression.MemberInit(xNew, bindings);

                // expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
                var lambda = Expression.Lambda(xInit, parameter);

                model.GetType().GetProperty("Select").SetValue(model, lambda);
            }
        }

        private static void ExtractPagination(object model, ModelBindingContext bindingContext)
        {
            var page = bindingContext.ValueProvider.GetValue("page") != null ? bindingContext.ValueProvider.GetValue("page").AttemptedValue : null;
            var pageSize = bindingContext.ValueProvider.GetValue("pagesize") != null ? bindingContext.ValueProvider.GetValue("pagesize").AttemptedValue : null;
            
            if (!string.IsNullOrWhiteSpace(page))
                model.GetType().GetProperty("Page").SetValue(model, int.Parse(page));

            if (!string.IsNullOrWhiteSpace(pageSize))
                model.GetType().GetProperty("PageSize").SetValue(model, int.Parse(pageSize));
        }

        private static void ExtractOrder(object model, ModelBindingContext bindingContext, ParameterExpression parameter)
        {
            var order = bindingContext.ValueProvider.GetValue("order") != null ? bindingContext.ValueProvider.GetValue("order").AttemptedValue : null;

            if (!string.IsNullOrWhiteSpace(order))
            {
                if (order.Split('=').Count() > 1)
                {
                    model.GetType().GetProperty("OrderType").SetValue(model, Enum.Parse(typeof(OrderType), order.Split('=')[1], true));
                    order = order.Split('=')[0];
                }
                else
                    model.GetType().GetProperty("OrderType").SetValue(model, OrderType.Asc);

                var property = Expression.PropertyOrField(parameter, order);

                var orderExp = Expression.Lambda(Expression.Convert(property, typeof(Object)).Reduce(), parameter);

                model.GetType().GetProperty("Order").SetValue(model, orderExp);
            }
        }

        private static void ExtractFilters(object model, ModelBindingContext bindingContext, ParameterExpression parameter, Type itemType)
        {
            var filter = bindingContext.ValueProvider.GetValue("filter") != null ? bindingContext.ValueProvider.GetValue("filter").AttemptedValue : null;

            if (filter == null)
                filter = bindingContext.ValueProvider.GetValue("query") != null? bindingContext.ValueProvider.GetValue("query").AttemptedValue : null;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var filterAndValues = filter.Split(',').ToArray();

                LambdaExpression finalExpression = null;
                Expression currentExpression = null;
                var item = Activator.CreateInstance(itemType, true);

                for (int i = 0; i < filterAndValues.Count(); i++)
                {
                    if (filterAndValues[i].Contains('|'))
                    {
                        var orExpression = new ExpressionParser();
                        var filterAndValue = orExpression.DefineOperation(filterAndValues[i], itemType);
                        var options = filterAndValue[1].Split('|');

                        for (int j = 0; j < options.Count(); j++)
                        {
                            var expression = GetExpression(parameter, itemType, $"{filterAndValue[0]}{orExpression.GetOperation()}{options[j]}");

                            if (j == 0)
                            {
                                if (currentExpression == null)
                                    currentExpression = expression;
                                else
                                    currentExpression = Expression.And(currentExpression, expression);
                            }
                            else
                            {
                                currentExpression = Expression.Or(currentExpression, expression);
                            }

                        }
                    }
                    else
                    {
                        Expression expression = GetExpression(parameter, itemType, filterAndValues[i]);

                        if (currentExpression == null)
                        {
                            currentExpression = expression;
                        }
                        else
                        {
                            currentExpression = Expression.And(currentExpression, expression);
                        }
                    }
                }

                finalExpression = Expression.Lambda(currentExpression, parameter);

                model.GetType().GetProperty("Filter").SetValue(model, finalExpression);
            }
        }

        private static Expression GetExpression(ParameterExpression parameter, Type itemType, string filterAndValue)
        {
            var expressionType = new ExpressionParser(filterAndValue, itemType);

            var expression = expressionType.GetExpression(parameter);
            return expression;
        }

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var model = Activator.CreateInstance(bindingContext.ModelType);

            var itemType = bindingContext.ModelType.GenericTypeArguments[0];

            var parameter = Expression.Parameter(itemType, "x");

            ExtractFilters(model, bindingContext, parameter, itemType);

            ExtractOrder(model, bindingContext, parameter);

            ExtractPagination(model, bindingContext);

            //bindingContext.Result = ModelBindingResult.Success(model);

            return model;
        }
    }
}

    
