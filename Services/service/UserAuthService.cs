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

        public async Task<(string accessToken, string refreshToken)?> AuthenticateUserAsync(string username, string password)
        {
            var user = await _userRepository.AuthenticateUserAsync(username);
            if (user == null) return null;

            bool isValid = PasswordHasher.VerifyPassword(password, user.PasswordHash);
            if (!isValid) return null;

            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshTokenEntity = _jwtTokenService.GenerateRefreshToken(user.Id);

            // Guardar refresh token en base de datos
            await _refreshTokenRepository.AddAsync(refreshTokenEntity);

            return (accessToken, refreshTokenEntity.Token);
        }
    }
}
