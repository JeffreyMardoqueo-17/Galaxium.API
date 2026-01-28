using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Galaxium.API.Entities;
using Galaxium.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Galaxium.API.Services.Service
{
    /// <summary>
    /// Service responsible for generating and validating JWT tokens
    /// (Access Tokens and Refresh Tokens).
    /// </summary>
    public sealed class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _options;

        /// <summary>
        /// Initializes a new instance of <see cref="JwtTokenService"/>.
        /// </summary>
        /// <param name="options">JWT configuration options.</param>
        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        // =====================================================
        // ACCESS TOKEN
        // =====================================================

        /// <summary>
        /// Generates a JWT access token for an authenticated user.
        /// </summary>
        /// <param name="user">Authenticated user entity.</param>
        /// <returns>Serialized JWT access token.</returns>
        public string GenerateAccessToken(User user)
        {

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (user.Role == null)
                throw new InvalidOperationException("User role is not loaded.");

            var claims = new List<Claim>
            {
                // Subject: User ID
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

                // Unique username
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),

                // User role (used by [Authorize(Roles = "...")])
                new Claim(ClaimTypes.Role, user.Role.Name),

                // Unique token identifier
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.Key)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // =====================================================
        // REFRESH TOKEN
        // =====================================================

        /// <summary>
        /// Generates a secure refresh token for a user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>Refresh token entity.</returns>
        public RefreshToken GenerateRefreshToken(int userId)
        {
            // Generate cryptographically secure random bytes
            var randomBytes = RandomNumberGenerator.GetBytes(64);

            return new RefreshToken
            {
                UserId = userId,
                Token = Convert.ToBase64String(randomBytes),
                ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };
        }

        // =====================================================
        // TOKEN VALIDATION (EXPIRED ACCESS TOKEN)
        // =====================================================

        /// <summary>
        /// Retrieves the claims principal from an expired access token.
        /// Used during refresh token flow.
        /// </summary>
        /// <param name="token">Expired JWT access token.</param>
        /// <returns>
        /// ClaimsPrincipal if token is valid; otherwise throws exception.
        /// </returns>
        /// <exception cref="SecurityTokenException">
        /// Thrown when token is invalid or tampered.
        /// </exception>
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,

                // ⚠️ IMPORTANT:
                // Lifetime validation is disabled to allow expired tokens
                ValidateLifetime = false,

                ValidIssuer = _options.Issuer,
                ValidAudience = _options.Audience,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_options.Key)
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(
                token,
                tokenValidationParameters,
                out SecurityToken securityToken
            );

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
