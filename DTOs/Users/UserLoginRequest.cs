using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.API.DTOs.Users
{
     public record UserLoginRequest(
        string Username,
        string Password
    );
}