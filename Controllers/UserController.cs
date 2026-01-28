using System.Threading.Tasks;
using Galaxium.API.DTOs.Users;
using Galaxium.API.Entities;
using Galaxium.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Galaxium.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserAuthService _userAuthService;

        public UserController(IUserAuthService userAuthService)
        {
            _userAuthService = userAuthService;
        }

        // POST: api/User/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserCreateRequest request)
        {
            // Validaciones básicas (puedes extenderlas)
            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Password is required.");

            // Mapear DTO a entidad
            var newUser = new User
            {
                FullName = request.FullName,
                Username = request.Username,
                RoleId = request.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userAuthService.CreateUserAsync(newUser, request.Password);

            var response = new UserResponse(
                createdUser.Id,
                createdUser.FullName,
                createdUser.Username,
                createdUser.RoleId,
                createdUser.IsActive,
                createdUser.CreatedAt
            );

            return CreatedAtAction(nameof(GetById), new { userId = response.Id }, response);
        }

        // POST: api/User/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var authResult = await _userAuthService.AuthenticateUserAsync(request.Username, request.Password);

            if (authResult == null)
                return Unauthorized("Invalid username or password.");

            var response = new AuthResponse(authResult.Value.accessToken, authResult.Value.refreshToken);

            return Ok(response);
        }

        // GET: api/User/{userId}
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetById(int userId)
        {
            // Podrías implementar este método en el servicio para obtener usuario por id
            // Aquí solo retorno 404 para ejemplo
            return NotFound();
        }
    }
}
