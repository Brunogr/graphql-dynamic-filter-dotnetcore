using Graphql.DynamicFilter.Exceptions;
using Graphql.DynamicFilter.WebApi.Test.Controllers;
using Graphql.DynamicFiltering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Graphql.DynamicFilter.Test.Unit
{
    public class WebApi_Spec
    {
        public WebApi_Spec()
        {

        }

        [Theory]
        [InlineData("name=Bruno")]
        [InlineData("name=Albert")]
        [InlineData("name=Fred,age=33")]
        [InlineData("age=23")]
        public async Task Must_Match_Specific_User(string query)
        {
            var binder = new DynamicFilterBinder();

            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "query", new StringValues(query) }
            });

            var vp = new QueryStringValueProvider(BindingSource.Query, queryCollection, CultureInfo.CurrentCulture);

            var context = GetBindingContext(vp, typeof(DynamicFilter<User>));
            
            await binder.BindModelAsync(context);

            var dynamicFilter = context.Result.Model as DynamicFilter<User>;

            var result = await new UserController().Get(dynamicFilter);

            Assert.Single(result);            
        }

        [Theory]
        [InlineData("name%a", 2)]
        [InlineData("age>=27", 4)]
        [InlineData("name%b,age>27", 1)]
        [InlineData("name%b", 2)]
        [InlineData("name%%B", 1)]
        [InlineData("birthdate>=01/01/1991", 3)]
        [InlineData("address.street%street", 5)]
        public async void Must_Match_Multiple_Users(string query, int count)
        {
            var binder = new DynamicFilterBinder();

            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "query", new StringValues(query) }
            });

            var vp = new QueryStringValueProvider(BindingSource.Query, queryCollection, CultureInfo.CurrentCulture);

            var context = GetBindingContext(vp, typeof(DynamicFilter<User>));

            await binder.BindModelAsync(context);

            var dynamicFilter = context.Result.Model as DynamicFilter<User>;

            var result = await new UserController().Get(dynamicFilter);

            Assert.Equal(count, result.Count);

        }


        [Theory]
        [InlineData("name%a","name=Asc","Albert")]
        [InlineData("name%a", "name=Desc", "Lucao")]
        public async void Must_Order_Values(string query, string order, string firstName)
        {
            var binder = new DynamicFilterBinder();

            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "query", new StringValues(query) },
                { $"order", new StringValues(order) }
            });

            var vp = new QueryStringValueProvider(BindingSource.Query, queryCollection, CultureInfo.CurrentCulture);

            var context = GetBindingContext(vp, typeof(DynamicFilter<User>));

            await binder.BindModelAsync(context);

            var dynamicFilter = context.Result.Model as DynamicFilter<User>;

            var result = await new UserController().Get(dynamicFilter);

            Assert.Equal(firstName, result.FirstOrDefault().Name);

        }

        [Fact]
        public async void Must_Throw_PropertyNotFoundException()
        {
            var binder = new DynamicFilterBinder();

            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "query", new StringValues("Nome=Bruno") }
            });

            var vp = new QueryStringValueProvider(BindingSource.Query, queryCollection, CultureInfo.CurrentCulture);

            var context = GetBindingContext(vp, typeof(DynamicFilter<User>));

            await Assert.ThrowsAsync<PropertyNotFoundException>(async () => await binder.BindModelAsync(context));
        }

        private static DefaultModelBindingContext GetBindingContext(IValueProvider valueProvider, Type modelType)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var bindingContext = new DefaultModelBindingContext
            {
                ModelMetadata = metadataProvider.GetMetadataForType(modelType),
                ModelName = modelType.Name,
                ModelState = new ModelStateDictionary(),
                ValueProvider = valueProvider,
            };
            return bindingContext;
        }
    }
}
