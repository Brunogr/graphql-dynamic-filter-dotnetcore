using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphql.DynamicFiltering;
using Microsoft.AspNetCore.Mvc;

namespace Graphql.DynamicFilter.WebApi.Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        List<User> users = new List<User>()
        {
            new User("Bruno", 27),
            new User("Fred", 33),
            new User("Albert", 37),
            new User("Lucao", 23),
            new User("Luide", 28)
        };
        // GET api/values
        [HttpGet]
        public Task<List<User>> Get(DynamicFilter<User> filter)
        {
            var result = this.users.Where(filter.Filter.Compile());

            if (filter.Order != null)
            {
                if (filter.OrderType == OrderType.Asc)
                    result = result.OrderBy(filter.Order.Compile());
                else
                    result = result.OrderByDescending(filter.Order.Compile());
            }

            return Task.FromResult(result.ToList());
        }
    }

    public class User
    {
        private User()
        {

        }

        public User(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public string Name { get; set; }
        public int Age { get; set; }
        private DateTime _birthDate;

        public DateTime BirthDate
        {
            get
            {
                return DateTime.Now.AddYears(-Age);
            }
            set
            {
                _birthDate = value;
            }
        }
    }
}
