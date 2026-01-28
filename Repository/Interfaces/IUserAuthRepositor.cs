using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;

namespace Galaxium.API.Repository.Interfaces
{
    public interface IUserAuthRepository
    {

        Task<User> CreateUserAsync(User newUser);
        Task<User> AuthenticateUserAsync(string username, string password);
        
    }
}