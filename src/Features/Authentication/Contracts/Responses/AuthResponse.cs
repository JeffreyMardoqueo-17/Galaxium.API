using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.API.DTOs.Users
{
 public record AuthResponse(
        string AccessToken,
        string RefreshToken
    );
}