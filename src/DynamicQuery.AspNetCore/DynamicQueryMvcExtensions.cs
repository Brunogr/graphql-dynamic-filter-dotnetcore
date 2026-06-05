using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicQuery.AspNetCore
{
    public static class DynamicQueryMvcExtensions
    {
        public static IMvcBuilder AddDynamicQuery(this IMvcBuilder builder)
        {
            builder.AddMvcOptions(options =>
            {
                options.ModelBinderProviders.Insert(0, new DynamicQueryBinderProvider());
            });

            return builder;
        }
    }
}
