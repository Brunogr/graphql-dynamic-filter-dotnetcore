using DynamicQuery.AspNetCore;
using DynamicQuery.AspNetCore.Test.Models;
using DynamicQuery.Parser;
using Xunit;

namespace DynamicQuery.Test.Unit
{
    public class DynamicQueryExtensionsTests
    {
        [Fact]
        public void Apply_On_IEnumerable_Throws_When_Source_Is_Null()
        {
            IEnumerable<User> source = null!;
            var query = new DynamicQuery<User>();

            Assert.Throws<ArgumentNullException>(() => source.Apply(query));
        }

        [Fact]
        public void Apply_On_IEnumerable_Throws_When_Query_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => UserData.Users.Apply(null!));
        }

        [Fact]
        public void Apply_On_IQueryable_Throws_When_Source_Is_Null()
        {
            IQueryable<User> source = null!;
            var query = new DynamicQuery<User>();

            Assert.Throws<ArgumentNullException>(() => source.Apply(query));
        }

        [Fact]
        public void Apply_On_IQueryable_Applies_Filter_Order_Select_And_Paging()
        {
            var query = QueryBindingEngine.Bind<User>(new Dictionary<string, string?>
            {
                ["query"] = "age>=23",
                ["order"] = "name=Asc",
                ["select"] = "name,age",
                ["page"] = "1",
                ["pagesize"] = "2"
            });

            var result = UserData.Users.AsQueryable().Apply(query).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("Bruno", result[0].Name);
            Assert.Equal(27, result[0].Age);
        }

        [Fact]
        public void Apply_On_IQueryable_Orders_Descending()
        {
            var query = QueryBindingEngine.Bind<User>(new Dictionary<string, string?>
            {
                ["query"] = "name%a",
                ["order"] = "name=Desc"
            });

            var result = UserData.Users.AsQueryable().Apply(query).ToList();

            Assert.Equal("Lucao", result[0].Name);
        }

        [Fact]
        public void Apply_On_IEnumerable_Applies_Select_And_Paging()
        {
            var query = QueryBindingEngine.Bind<User>(new Dictionary<string, string?>
            {
                ["order"] = "name=Asc",
                ["page"] = "0",
                ["pagesize"] = "1"
            });

            var result = UserData.Users.Apply(query).ToList();

            Assert.Single(result);
            Assert.Equal("Albert", result[0].Name);
        }
    }
}
