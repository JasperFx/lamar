using System.Collections.Generic;
using System.Linq;
using JasperFx.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UserApp.Controllers
{
    public class User
    {
        public string Name { get; set; }

        protected bool Equals(User other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }



    public class UserController : Controller
    {
        private readonly UserRepository _repository;
        private readonly ILogger<User> _logger;

        public UserController(UserRepository repository, ILogger<User> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpPost("/user/create")]
        public IActionResult Create([FromBody] User user)
        {
            _logger.LogInformation($"I created a new user with the name '{user.Name}'");
            _repository.Users.Fill(user);
            return Ok();
        }
        
        [HttpGet("/user/{name}")]
        public User GetUser(string name)
        {
            return _repository.Users.FirstOrDefault(x => x.Name == name);
        }

        [HttpGet("/users")]
        public User[] GetUsers()
        {
            return _repository.Users.ToArray();
        }
    }
    
        
    public class UserRepository
    {
        public readonly IList<User> Users = new List<User>();

        public UserRepository()
        {
            Users.Add(new User{Name = "Luke"});
            Users.Add(new User{Name = "Leia"});
        }
    }
}