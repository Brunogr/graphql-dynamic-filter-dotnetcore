using DynamicQuery.Parser;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DynamicQuery.AspNetCore
{
    public partial class DynamicQuery<T>
    {
        public static ValueTask<DynamicQuery<T>?> BindAsync(HttpContext httpContext, ParameterInfo parameter)
        {
            var queryParams = httpContext.Request.Query.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Count == 0 ? null : pair.Value.ToString(),
                StringComparer.OrdinalIgnoreCase);

            var model = QueryBindingEngine.Bind<T>(queryParams);
            return ValueTask.FromResult<DynamicQuery<T>?>(DynamicQueryBinder.FromModel(model));
        }
    }
}
