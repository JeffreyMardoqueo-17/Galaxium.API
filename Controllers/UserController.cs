using System.Threading.Tasks;
using AutoMapper;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.DTOs.Users;
using Galaxium.API.Entities;
using Galaxium.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxium.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserAuthService _userAuthService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(
            IUserAuthService userAuthService,
            IUserService userService,
            IMapper mapper)
        {
            _userAuthService = userAuthService;
            _userService = userService;
            _mapper = mapper;
        }

        // POST: api/User/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Password is required.");

            //DTO -> Entity (AutoMapper)
            var newUser = _mapper.Map<User>(request);

            var createdUser = await _userAuthService.CreateUserAsync(
                newUser,
                request.Password
            );

            //Entity -> DTO (AutoMapper)
            var response = _mapper.Map<UserResponse>(createdUser);

            return CreatedAtAction(
                nameof(GetById),
                new { userId = response.Id },
                response
            );
        }

        // POST: api/User/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var authResult = await _userAuthService.AuthenticateUserAsync(
                request.Username,
                request.Password
            );

            if (authResult == null)
                return Unauthorized("Invalid username or password.");

            // Tuple -> AuthResponse (AutoMapper)
            var response = _mapper.Map<AuthResponse>(authResult.Value);

            return Ok(response);
        }

        // GET: api/User/{userId}
        [HttpGet("{userId:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            // Entity -> DTO (AutoMapper)
            var response = _mapper.Map<UserResponse>(user);
            return Ok(response);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();

            // Entity List -> DTO List (AutoMapper)
            var response = _mapper.Map<List<UserResponse>>(users);
            return Ok(response);
        }
    }
}
