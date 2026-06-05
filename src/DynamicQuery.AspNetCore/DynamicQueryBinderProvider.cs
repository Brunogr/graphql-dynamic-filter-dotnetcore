using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;

namespace DynamicQuery.AspNetCore
{
    public class DynamicQueryBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType.IsGenericType &&
                context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(DynamicQuery<>))
            {
                return new BinderTypeModelBinder(typeof(DynamicQueryBinder));
            }

            return null;
        }
    }
}
