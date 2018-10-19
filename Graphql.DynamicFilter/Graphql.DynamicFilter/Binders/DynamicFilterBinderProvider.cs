using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Graphql.DynamicFiltering
{
    public class DynamicFilterBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType.FullName.Contains("DynamicFilter"))
            {
                return new BinderTypeModelBinder(typeof(DynamicFilterBinder));
            }

            return null;
        }
    }
}
