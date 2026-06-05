using DynamicQuery.AspNetCore;
using DynamicQuery.AspNetCore.Test.Models;
using DynamicQuery.Parser;
using DynamicQuery.Parser.Exceptions;
using Xunit;

namespace DynamicQuery.Test.Unit
{
    public class QueryBindingEngineTests
    {
        [Fact]
        public void Bind_Uses_Filter_Parameter_When_Present()
        {
            var query = QueryBindingEngine.Bind<User>(new Dictionary<string, string?>
            {
                ["filter"] = "name=Bruno"
            });

            var result = UserData.Users.Apply(query).ToList();
            Assert.Single(result);
        }

        [Fact]
        public void Bind_Applies_Pagination()
        {
            var query = QueryBindingEngine.Bind<User>(new Dictionary<string, string?>
            {
                ["query"] = "age>=23",
                ["order"] = "name=Asc",
                ["page"] = "1",
                ["pagesize"] = "2"
            });

            var result = UserData.Users.Apply(query).ToList();
            Assert.Equal(2, result.Count);
            Assert.Equal("Bruno", result[0].Name);
        }

        [Fact]
        public void Bind_Applies_Select_Projection()
        {
            var query = QueryBindingEngine.Bind<User>(new Dictionary<string, string?>
            {
                ["query"] = "name%a",
                ["select"] = "name,age"
            });

            Assert.Equal("name,age", query.SelectText);
            Assert.NotNull(query.Select);

            var result = UserData.Users.Apply(query).ToList();
            Assert.All(result, user =>
            {
                Assert.Contains("a", user.Name, StringComparison.OrdinalIgnoreCase);
                Assert.NotNull(user.Name);
            });
        }

        [Fact]
        public void Bind_Applies_Or_Filters()
        {
            var query = QueryBindingEngine.Bind<User>(new Dictionary<string, string?>
            {
                ["query"] = "name=Bruno|Albert"
            });

            var result = UserData.Users.Apply(query).ToList();
            Assert.Equal(2, result.Count);
            Assert.Contains(result, user => user.Name == "Bruno");
            Assert.Contains(result, user => user.Name == "Albert");
        }

        [Fact]
        public void Bind_Uses_ValueProvider_Overload()
        {
            var query = QueryBindingEngine.Bind<User>(key => key switch
            {
                "query" => "name=Lucao",
                "ORDER" => null,
                _ => null
            });

            var result = UserData.Users.Apply(query).ToList();
            Assert.Single(result);
            Assert.Equal("Lucao", result[0].Name);
        }

        [Fact]
        public void Bind_Resolves_Query_Parameters_Case_Insensitively()
        {
            var query = QueryBindingEngine.Bind<User>(new Dictionary<string, string?>
            {
                ["QUERY"] = "name=Luide"
            });

            var result = UserData.Users.Apply(query).ToList();
            Assert.Single(result);
        }

        [Fact]
        public void Bind_Throws_When_Select_Field_Is_Unknown()
        {
            Assert.Throws<PropertyNotFoundException>(() =>
                QueryBindingEngine.Bind<User>(new Dictionary<string, string?>
                {
                    ["select"] = "unknownField"
                }));
        }

        [Theory]
        [InlineData("age<30", 3)]
        [InlineData("age!=27", 4)]
        [InlineData("age<=28", 3)]
        public void Bind_Supports_Comparison_Operators(string filter, int expectedCount)
        {
            var query = QueryBindingEngine.Bind<User>(new Dictionary<string, string?>
            {
                ["query"] = filter
            });

            var result = UserData.Users.Apply(query).ToList();
            Assert.Equal(expectedCount, result.Count);
        }

        [Fact]
        public void Bind_Supports_Nullable_Property_Filter()
        {
            var query = QueryBindingEngine.Bind<User>(new Dictionary<string, string?>
            {
                ["query"] = "address.number=23"
            });

            var result = UserData.Users.Apply(query).ToList();
            Assert.Single(result);
            Assert.Equal("Bruno", result[0].Name);
        }

        [Fact]
        public void Bind_Supports_Collection_Any_Filter()
        {
            var query = QueryBindingEngine.Bind<User>(new Dictionary<string, string?>
            {
                ["query"] = "roles.name=Write"
            });

            var result = UserData.Users.Apply(query).ToList();
            Assert.Equal(3, result.Count);
        }
    }
}
