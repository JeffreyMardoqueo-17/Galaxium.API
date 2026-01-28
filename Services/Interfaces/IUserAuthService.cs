using System.Threading.Tasks;
using Galaxium.API.Entities;

namespace Galaxium.API.Services.Interfaces
{
    public interface IUserAuthService
    {
        Task<User> CreateUserAsync(User newUser, string password);
        Task<(string accessToken, string refreshToken)?> AuthenticateUserAsync(string username, string password);
    }
}
