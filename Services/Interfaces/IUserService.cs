using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;

namespace Galaxium.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int userId);
        Task<User> UpdateUserRoleAsync(int userId, int roleId);
        Task<User> UpdateUserStatusAsync(int userId, bool isActive);

        /// <summary>
        /// Devuelve true si existe al menos un usuario en la base de datos.
        /// </summary>
        Task<bool> AnyUserExistsAsync();
    }
}