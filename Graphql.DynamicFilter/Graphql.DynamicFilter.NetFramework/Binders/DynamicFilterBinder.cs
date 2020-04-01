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

    
