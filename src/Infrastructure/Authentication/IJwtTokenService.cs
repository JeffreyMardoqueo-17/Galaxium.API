using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Galaxium.API.Entities;

namespace Galaxium.API.Services.Interfaces
{
    public interface IJwtTokenService
    {
                string GenerateAccessToken(User user);
        RefreshToken GenerateRefreshToken(int userId);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}