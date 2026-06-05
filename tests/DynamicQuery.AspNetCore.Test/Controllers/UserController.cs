using DynamicQuery.AspNetCore;
using DynamicQuery.AspNetCore.Test.Models;
using Microsoft.AspNetCore.Mvc;

namespace DynamicQuery.AspNetCore.Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet]
        public Task<List<User>> Get(DynamicQuery<User> query)
        {
            var result = UserData.Users.Apply(query).ToList();
            return Task.FromResult(result);
        }
    }
}
