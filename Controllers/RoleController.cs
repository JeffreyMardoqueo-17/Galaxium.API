using Galaxium.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Galaxium.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: api/role
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles); // 200
        }

        // GET: api/role/5
        [HttpGet("{roleId:int}")]
        public async Task<IActionResult> GetById(int roleId)
        {
            var role = await _roleService.GetRoleByIdAsync(roleId);
            return Ok(role); // 200
        }
    }
}
