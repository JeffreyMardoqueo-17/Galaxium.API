using System.Security.Claims;
using System.Threading.Tasks;
using Galaxium.API.Entities;
using Galaxium.API.Repository.Interfaces;
using Galaxium.API.Services.Interfaces;
using Galaxium.API.Utils;

namespace Galaxium.API.Services.Service
{
    public class UserAuthService : IUserAuthService
    {
        private readonly IUserAuthRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public UserAuthService(
            IUserAuthRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<User> CreateUserAsync(User newUser, string password)
        {
            var hash = PasswordHasher.HashPassword(password);
            newUser.PasswordHash = hash;
            // No salt, porque tu diseño actual no usa salt
            return await _userRepository.CreateUserAsync(newUser);
        }

        public async Task<(User user, string accessToken, string refreshToken)?>
  AuthenticateUserAsync(string username, string password)
        {
            var user = await _userRepository.AuthenticateUserAsync(username);
            if (user == null) return null;

            bool isValid = PasswordHasher.VerifyPassword(password, user.PasswordHash);
            if (!isValid) return null;

            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshTokenEntity = _jwtTokenService.GenerateRefreshToken(user.Id);

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);

            return (user, accessToken, refreshTokenEntity.Token);
        }

        //otros metodos para que funcione el login 
        public async Task<(string accessToken, string refreshToken)?> RefreshTokenAsync(
    string expiredAccessToken,
    string refreshToken
)
        {
            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(expiredAccessToken);
            if (principal == null)
                return null;

            var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));

            var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (storedToken == null ||
                storedToken.IsRevoked ||
                storedToken.ExpiresAt < DateTime.UtcNow ||
                storedToken.UserId != userId)
            {
                return null;
            }

            // Revocar token viejo
            storedToken.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(storedToken);

            // Crear nuevos
            var user = storedToken.User!;
            var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

            await _refreshTokenRepository.AddAsync(newRefreshToken);

            return (newAccessToken, newRefreshToken.Token);
        }
        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token == null) return;

            token.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(token);
        }

    }
}
