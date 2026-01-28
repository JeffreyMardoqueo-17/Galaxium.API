using Galaxium.API.Common;
using Galaxium.API.Entities;
using Galaxium.API.Repository.Interfaces;
using Galaxium.API.Services.Interfaces;

namespace Galaxium.API.Services.service
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRespository _roleRespository;

        public RoleService(IRoleRespository roleRespository)
        {
            _roleRespository = roleRespository;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            var roles = await _roleRespository.GetAllRolesAsync();

            if (roles == null || !roles.Any())
                throw new NotFoundBusinessException("No roles found.");

            return roles;
        }

        public async Task<Role> GetRoleByIdAsync(int roleId)
        {
            if (roleId <= 0)
                throw new BusinessException("Invalid role ID provided.");

            var role = await _roleRespository.GetRoleByIdAsync(roleId);

            if (role == null)
                throw new NotFoundBusinessException($"Role with ID {roleId} not found.");

            return role;
        }
    }
}
