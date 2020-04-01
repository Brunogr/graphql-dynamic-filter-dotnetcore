using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Graphql.NetFramework.DynamicFiltering
{
    public class DynamicFilterBinderProvider : IModelBinderProvider
    {

        public IModelBinder GetBinder(Type modelType)
        {
            if (modelType == null)
            {
                throw new ArgumentNullException(nameof(modelType));
            }

            if (modelType.FullName.Contains("DynamicFilter"))
            {
                return new DynamicFilterBinder();
            }

            return null;
        }
    }
}
