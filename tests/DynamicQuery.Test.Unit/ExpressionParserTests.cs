using DynamicQuery.Parser;
using System.Linq.Expressions;
using Xunit;

namespace DynamicQuery.Test.Unit
{
    public class ExpressionParserTests
    {
        [Fact]
        public void Parses_Guid_Property()
        {
            var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var items = new[]
            {
                new GuidItem { Id = id },
                new GuidItem { Id = Guid.NewGuid() }
            };

            var query = QueryBindingEngine.Bind<GuidItem>(new Dictionary<string, string?>
            {
                ["query"] = $"id={id}"
            });

            var result = items.Where(query.Filter!.Compile()).ToList();
            Assert.Single(result);
        }

        [Fact]
        public void GetOperation_Returns_All_Operator_Tokens()
        {
            var parser = new ExpressionParser("name=test", typeof(GuidItem));
            Assert.Equal("=", parser.GetOperation());

            parser = new ExpressionParser("name%test", typeof(GuidItem));
            Assert.Equal("%", parser.GetOperation());

            parser = new ExpressionParser("name%%Test", typeof(GuidItem));
            Assert.Equal("%%", parser.GetOperation());

            parser = new ExpressionParser("age>1", typeof(GuidItem));
            Assert.Equal(">", parser.GetOperation());

            parser = new ExpressionParser("age<1", typeof(GuidItem));
            Assert.Equal("<", parser.GetOperation());

            parser = new ExpressionParser("age>=1", typeof(GuidItem));
            Assert.Equal(">=", parser.GetOperation());

            parser = new ExpressionParser("age<=1", typeof(GuidItem));
            Assert.Equal("<=", parser.GetOperation());

            parser = new ExpressionParser("age!=1", typeof(GuidItem));
            Assert.Equal("!=", parser.GetOperation());
        }

        private sealed class GuidItem
        {
            public Guid Id { get; set; }
            public int Age { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
