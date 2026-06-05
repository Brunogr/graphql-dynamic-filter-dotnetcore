using DynamicQuery.Parser;
using System;
using System.Web.Mvc;

namespace DynamicQuery.NetFramework
{
    public class DynamicQueryBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var itemType = bindingContext.ModelType.GetGenericArguments()[0];
            var bindMethod = typeof(DynamicQueryBinder)
                .GetMethod(nameof(BindInternal), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(itemType);

            return bindMethod.Invoke(null, new object[] { bindingContext })!;
        }

        private static DynamicQuery<T> BindInternal<T>(ModelBindingContext bindingContext)
        {
            var model = QueryBindingEngine.Bind<T>(key =>
            {
                var value = bindingContext.ValueProvider.GetValue(key);
                return value?.AttemptedValue;
            });

            return new DynamicQuery<T>
            {
                Filter = model.Filter,
                Order = model.Order,
                Select = model.Select,
                SelectText = model.SelectText,
                OrderType = model.OrderType,
                Page = model.Page,
                PageSize = model.PageSize
            };
        }
    }
}
