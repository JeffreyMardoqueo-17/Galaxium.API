using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.API.Entities;


namespace Galaxium.API.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(int roleId);
    }
}