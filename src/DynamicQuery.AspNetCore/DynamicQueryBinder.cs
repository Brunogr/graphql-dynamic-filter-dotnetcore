using DynamicQuery.Parser;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace DynamicQuery.AspNetCore
{
    public class DynamicQueryBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var itemType = bindingContext.ModelType.GetGenericArguments()[0];
            var bindMethod = typeof(DynamicQueryBinder)
                .GetMethod(nameof(BindInternal), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(itemType);

            object model;
            try
            {
                model = bindMethod.Invoke(null, new object[] { bindingContext })!;
            }
            catch (System.Reflection.TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }

            bindingContext.Result = ModelBindingResult.Success(model);

            return Task.CompletedTask;
        }

        private static DynamicQuery<T> BindInternal<T>(ModelBindingContext bindingContext)
        {
            var model = QueryBindingEngine.Bind<T>(key => bindingContext.ValueProvider.GetValue(key).FirstValue);
            return FromModel(model);
        }

        internal static DynamicQuery<T> FromModel<T>(DynamicQueryModel<T> model)
        {
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
