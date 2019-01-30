using System.Threading.Tasks;
using UserApp.Controllers;

namespace LamarRest.Testing
{
    public interface IUserService
    {
        [Get("/user/{name}")]
        Task<User> GetUser(string name);

        [Post("/user/create")]
        Task Create(User user);
    }
}