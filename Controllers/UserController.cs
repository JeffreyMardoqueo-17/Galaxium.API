using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.DTOs.Users;
using Galaxium.API.Entities;
using Galaxium.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;

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

            var (user, accessToken, refreshToken) = authResult.Value;

            var userResponse = _mapper.Map<UserResponse>(user);
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"{claim.Type} = {claim.Value}");
            }
            return Ok(new
            {
                accessToken,
                refreshToken,
                user = userResponse
            });

        }


        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("Token inválido: no contiene UserId");

            var userId = int.Parse(userIdClaim.Value);

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound();

            var response = _mapper.Map<UserResponse>(user);
            return Ok(response);
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var accessToken = Request.Cookies["access_token"];
            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var result = await _userAuthService.RefreshTokenAsync(
                accessToken,
                refreshToken
            );

            if (result == null)
                return Unauthorized();

            var (newAccessToken, newRefreshToken) = result.Value;

            Response.Cookies.Append("access_token", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });

            Response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(30)
            });

            return Ok();
        }
        [HttpPost("logout")]
        //[Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _userAuthService.RevokeRefreshTokenAsync(refreshToken);
            }

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");

            return Ok();
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
