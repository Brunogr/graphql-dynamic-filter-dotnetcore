using Graphql.DynamicFiltering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Graphql.DynamicFiltering
{
    public class DynamicFilterBinder : IModelBinder
    {
        public DynamicFilterBinder()
        {
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
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

            bindingContext.Result = ModelBindingResult.Success(model);
        }

        private static void ExtractPagination(object model, ModelBindingContext bindingContext)
        {
            var page = bindingContext.ValueProvider.GetValue("page").FirstValue;
            var pageSize = bindingContext.ValueProvider.GetValue("pagesize").FirstValue;
            
            if (!string.IsNullOrWhiteSpace(page))
                model.GetType().GetProperty("Page").SetValue(model, int.Parse(page));

            if (!string.IsNullOrWhiteSpace(pageSize))
                model.GetType().GetProperty("PageSize").SetValue(model, int.Parse(pageSize));
        }

        private static void ExtractOrder(object model, ModelBindingContext bindingContext, ParameterExpression parameter)
        {
            var order = bindingContext.ValueProvider.GetValue("order").FirstValue;

            if (!string.IsNullOrWhiteSpace(order))
            {
                if (order.Split('=').Count() > 1)
                {
                    model.GetType().GetProperty("OrderType").SetValue(model, Enum.Parse(typeof(OrderType), order.Split('=')[1], true));
                    order = order.Split('=')[0];
                }
                else
                    model.GetType().GetProperty("OrderType").SetValue(model, OrderType.Desc);

                var property = Expression.PropertyOrField(parameter, order);

                var orderExp = Expression.Lambda(Expression.Convert(property, typeof(Object)).Reduce(), parameter);

                model.GetType().GetProperty("Order").SetValue(model, orderExp);
            }
        }

        private static void ExtractFilters(object model, ModelBindingContext bindingContext, ParameterExpression parameter, Type itemType)
        {
            var filter = bindingContext.ValueProvider.GetValue("filter").FirstValue;

            if (filter == null)
                filter = bindingContext.ValueProvider.GetValue("query").FirstValue;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var filterAndValues = filter.Split(',').ToArray();

                LambdaExpression finalExpression = null;
                Expression currentExpression = null;
                var item = Activator.CreateInstance(itemType, true);

                for (int i = 0; i < filterAndValues.Count(); i++)
                {
                    var expressionType = new ExpressionParser(filterAndValues[i], itemType);

                    var expression = expressionType.GetExpression(parameter);

                    if (currentExpression == null)
                    {
                        currentExpression = expression;
                    }
                    else
                    {
                        currentExpression = Expression.And(currentExpression, expression);
                    }
                }

                finalExpression = Expression.Lambda(currentExpression, parameter);

                model.GetType().GetProperty("Filter").SetValue(model, finalExpression);
            }
        }

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

    }
}

    
