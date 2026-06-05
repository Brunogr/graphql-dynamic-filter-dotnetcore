using System;
using System.Web.Mvc;

namespace DynamicQuery.NetFramework
{
    public class DynamicQueryBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(Type modelType)
        {
            if (modelType == null)
                throw new ArgumentNullException(nameof(modelType));

            if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(DynamicQuery<>))
                return new DynamicQueryBinder();

            return null;
        }
    }
}
