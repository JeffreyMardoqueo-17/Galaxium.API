using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.API.DTOs.Users
{
    public record RefreshTokenResponse(
       string Token,
       DateTime ExpiresAt,
       bool IsRevoked
   );
}