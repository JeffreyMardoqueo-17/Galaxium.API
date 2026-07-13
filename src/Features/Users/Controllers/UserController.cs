using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Galaxium.Api.Features.Tenants.Services;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.DTOs.Users;
using Galaxium.API.Entities;
using Galaxium.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Galaxium.Api.Utils;

namespace Galaxium.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private const string AccessTokenCookieName = "access_token";
        private const string RefreshTokenCookieName = "refresh_token";

        private readonly IUserAuthService _userAuthService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IPasswordResetService _passwordResetService;
        private readonly IWebHostEnvironment _environment;
        private readonly ITenantService _tenantService;

        public UserController(
            IUserAuthService userAuthService,
            IUserService userService,
            IMapper mapper,
            IPasswordResetService passwordResetService,
            IWebHostEnvironment environment,
            ITenantService tenantService)
        {
            _userAuthService = userAuthService;
            _userService = userService;
            _mapper = mapper;
            _passwordResetService = passwordResetService;
            _environment = environment;
            _tenantService = tenantService;
        }

        // POST: api/User/first-register
        /// <summary>
        /// Permite crear el primer tenant y su usuario administrador SOLO si no existe ningún usuario en la base.
        /// </summary>
        [HttpPost("first-register")]
        [AllowAnonymous]
        public async Task<IActionResult> FirstRegister([FromBody] FirstRegisterRequest request)
        {
            if (await _userService.AnyUserExistsAsync())
                return StatusCode(403, "Ya existe al menos un usuario registrado. Este endpoint solo puede usarse una vez.");

            if (string.IsNullOrWhiteSpace(request.TenantName))
                return BadRequest("Tenant name is required.");

            if (string.IsNullOrWhiteSpace(request.FullName))
                return BadRequest("Full name is required.");

            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest("Username is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Password is required.");

            // Crear Tenant usando el servicio dedicado (no DbContext directamente)
            var tenant = await _tenantService.CreateAsync(
                request.TenantName.Trim(),
                null,
                null,
                null,
                null,
                50,
                1000);

            // Crear usuario administrador
            var newUser = _mapper.Map<User>(new UserCreateRequest(
                request.FullName,
                request.Username,
                request.Password,
                1));

            newUser.RoleId = 1;
            newUser.TenantId = tenant.Id;

            var createdUser = await _userAuthService.CreateUserAsync(newUser, request.Password);

            var response = _mapper.Map<UserResponse>(createdUser);

            return CreatedAtAction(
                nameof(GetById),
                new { userId = response.Id },
                response);
        }

        // POST: api/User/register
        [HttpPost("register")]
        [Authorize(Policy = GalaxiumPolicyNames.AdminOnly)]
        public async Task<IActionResult> Register([FromBody] UserCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Password is required.");

            if (!TryGetTenantIdFromClaims(out var tenantId))
                return Unauthorized("Token inválido: no contiene TenantId");

            var newUser = _mapper.Map<User>(request);
            newUser.TenantId = tenantId;

            var createdUser = await _userAuthService.CreateUserAsync(newUser, request.Password);

            var response = _mapper.Map<UserResponse>(createdUser);

            return CreatedAtAction(
                nameof(GetById),
                new { userId = response.Id },
                response);
        }

        // POST: api/User/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var authResult = await _userAuthService.AuthenticateUserAsync(
                request.Username,
                request.Password);

            if (authResult == null)
                return Unauthorized("Invalid username or password.");

            var (user, accessToken, refreshToken) = authResult.Value;
            SetAuthCookies(accessToken, refreshToken);

            var userResponse = _mapper.Map<UserResponse>(user);
            return Ok(new { user = userResponse });
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
            if (user == null) return NotFound();

            var response = _mapper.Map<UserResponse>(user);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var accessToken = Request.Cookies[AccessTokenCookieName];
            var refreshToken = Request.Cookies[RefreshTokenCookieName];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var result = await _userAuthService.RefreshTokenAsync(accessToken, refreshToken);
            if (result == null) return Unauthorized();

            var (newAccessToken, newRefreshToken) = result.Value;
            SetAuthCookies(newAccessToken, newRefreshToken);

            return Ok();
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _userAuthService.RevokeRefreshTokenAsync(refreshToken);
            }

            DeleteAuthCookies();
            return Ok();
        }

        // GET: api/User/{userId}
        [HttpGet("{userId:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            // MultiTenant: ensure the user belongs to the current tenant
            if (!TryGetTenantIdFromClaims(out var currentTenantId) || user.TenantId != currentTenantId)
                return Forbid();

            var response = _mapper.Map<UserResponse>(user);
            return Ok(response);
        }

        [HttpGet]
        [Authorize(Policy = GalaxiumPolicyNames.AdminOrSupervisor)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            var response = _mapper.Map<List<UserResponse>>(users);
            return Ok(response);
        }

        [HttpPatch("{userId:int}/role")]
        [Authorize(Policy = GalaxiumPolicyNames.AdminOnly)]
        public async Task<IActionResult> UpdateRole(int userId, [FromBody] UserUpdateRoleRequest request)
        {
            var updated = await _userService.UpdateUserRoleAsync(userId, request.RoleId);
            var response = _mapper.Map<UserResponse>(updated);
            return Ok(response);
        }

        [HttpPatch("{userId:int}/status")]
        [Authorize(Policy = GalaxiumPolicyNames.AdminOnly)]
        public async Task<IActionResult> UpdateStatus(int userId, [FromBody] UserUpdateStatusRequest request)
        {
            var updated = await _userService.UpdateUserStatusAsync(userId, request.IsActive);
            var response = _mapper.Map<UserResponse>(updated);
            return Ok(response);
        }

        // POST: api/User/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _passwordResetService.SendResetCodeAsync(request.Email);
            return Ok(new { message = "Si el correo está registrado, recibirás un código de verificación." });
        }

        // POST: api/User/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                return BadRequest(new { message = "La nueva contraseña debe tener al menos 6 caracteres." });

            var success = await _passwordResetService.ResetPasswordAsync(
                request.Email, request.Code, request.NewPassword);

            if (!success)
                return BadRequest(new { message = "El código es inválido o ya expiró." });

            return Ok(new { message = "Contraseña actualizada exitosamente." });
        }

        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            Response.Cookies.Append(
                AccessTokenCookieName,
                accessToken,
                BuildCookieOptions(DateTime.UtcNow.AddHours(12)));

            Response.Cookies.Append(
                RefreshTokenCookieName,
                refreshToken,
                BuildCookieOptions(DateTime.UtcNow.AddDays(30)));
        }

        private void DeleteAuthCookies()
        {
            var deleteOptions = BuildCookieOptions(DateTime.UtcNow.AddDays(-1));
            Response.Cookies.Delete(AccessTokenCookieName, deleteOptions);
            Response.Cookies.Delete(RefreshTokenCookieName, deleteOptions);
        }

        private bool TryGetTenantIdFromClaims(out int tenantId)
        {
            var tenantIdClaim = User.FindFirst("tenant_id")?.Value
                ?? User.FindFirst("TenantId")?.Value;

            if (int.TryParse(tenantIdClaim, out var parsedTenantId) && parsedTenantId > 0)
            {
                tenantId = parsedTenantId;
                return true;
            }

            tenantId = 0;
            return false;
        }

        private CookieOptions BuildCookieOptions(DateTime expiresUtc)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = expiresUtc
            };
        }
    }
}
