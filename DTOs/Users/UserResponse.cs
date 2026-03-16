using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.API.DTOs.Users
{
    public record UserResponse(
        int Id,
        string FullName,
        string Username,
        int RoleId,
        string RoleName,
        bool IsActive,
        DateTime CreatedAt
    );
}