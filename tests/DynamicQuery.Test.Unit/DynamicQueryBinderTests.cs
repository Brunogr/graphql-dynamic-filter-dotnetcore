using DynamicQuery.AspNetCore;
using DynamicQuery.AspNetCore.Test.Controllers;
using DynamicQuery.AspNetCore.Test.Models;
using DynamicQuery.Parser;
using DynamicQuery.Parser.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using System.Globalization;
using Xunit;

namespace DynamicQuery.Test.Unit
{
    public class DynamicQueryBinderTests
    {
        [Theory]
        [InlineData("name=Bruno")]
        [InlineData("name=Albert")]
        [InlineData("name=Fred,age=33")]
        [InlineData("age=23")]
        public async Task Must_Match_Specific_User(string query)
        {
            var dynamicQuery = await BindQueryAsync(query);
            var result = await new UserController().Get(dynamicQuery);
            Assert.Single(result);
        }

        [Theory]
        [InlineData("name%a", 2)]
        [InlineData("age>=27", 4)]
        [InlineData("name%b,age>27", 1)]
        [InlineData("name%b", 2)]
        [InlineData("name%%B", 1)]
        [InlineData("age<=28", 3)]
        [InlineData("address.street%street", 5)]
        public async Task Must_Match_Multiple_Users(string query, int count)
        {
            var dynamicQuery = await BindQueryAsync(query);
            var result = await new UserController().Get(dynamicQuery);
            Assert.Equal(count, result.Count);
        }

        [Theory]
        [InlineData("name%a", "name=Asc", "Albert")]
        [InlineData("name%a", "name=Desc", "Lucao")]
        public async Task Must_Order_Values(string query, string order, string firstName)
        {
            var dynamicQuery = await BindQueryAsync(query, order);
            var result = await new UserController().Get(dynamicQuery);
            Assert.Equal(firstName, result.FirstOrDefault()?.Name);
        }

        [Fact]
        public async Task Must_Throw_PropertyNotFoundException()
        {
            var binder = new DynamicQueryBinder();
            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "query", new StringValues("Nome=Bruno") }
            });

            var valueProvider = new QueryStringValueProvider(
                BindingSource.Query,
                queryCollection,
                CultureInfo.CurrentCulture);

            var context = GetBindingContext(valueProvider, typeof(DynamicQuery<User>));

            await Assert.ThrowsAsync<PropertyNotFoundException>(() => binder.BindModelAsync(context));
        }

        [Fact]
        public void QueryBindingEngine_Binds_From_Dictionary()
        {
            var query = QueryBindingEngine.Bind<User>(new Dictionary<string, string?>
            {
                ["query"] = "name=Bruno"
            });

            var result = UserData.Users.Apply(query).ToList();
            Assert.Single(result);
            Assert.Equal("Bruno", result[0].Name);
        }

        [Fact]
        public async Task BindAsync_Parses_MinimalApiQuery()
        {
            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString("?query=name=Albert");

            var query = await DynamicQuery<User>.BindAsync(context, null!);

            Assert.NotNull(query);
            var result = UserData.Users.Apply(query).ToList();
            Assert.Single(result);
            Assert.Equal("Albert", result[0].Name);
        }

        private static async Task<DynamicQuery<User>> BindQueryAsync(string query, string? order = null)
        {
            var binder = new DynamicQueryBinder();
            var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "query", new StringValues(query) }
            });

            if (order != null)
                queryCollection = new QueryCollection(new Dictionary<string, StringValues>
                {
                    { "query", new StringValues(query) },
                    { "order", new StringValues(order) }
                });

            var valueProvider = new QueryStringValueProvider(
                BindingSource.Query,
                queryCollection,
                CultureInfo.CurrentCulture);

            var context = GetBindingContext(valueProvider, typeof(DynamicQuery<User>));
            await binder.BindModelAsync(context);
            return (DynamicQuery<User>)context.Result.Model!;
        }

        private static DefaultModelBindingContext GetBindingContext(IValueProvider valueProvider, Type modelType)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            return new DefaultModelBindingContext
            {
                ModelMetadata = metadataProvider.GetMetadataForType(modelType),
                ModelName = modelType.Name,
                ModelState = new ModelStateDictionary(),
                ValueProvider = valueProvider,
            };
        }
    }
}
