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
            new User("Bruno", 27, new Address("street 1", 23)),
            new User("Fred", 33, new Address("street 2", null)),
            new User("Albert", 37, new Address("street 3", 43)),
            new User("Lucao", 23, new Address("street 4", 53)),
            new User("Luide", 28, new Address("street 5", 63))
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

        public User(string name, int age, Address address)
        {
            Name = name;
            Age = age;
            Address = address;
        }

        public string Name { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }

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

    public class Address
    {
        public Address(string street, int? number)
        {
            Street = street;
            Number = number;
        }

        public string Street { get; set; }
        public int? Number { get; set; }
    }
}
