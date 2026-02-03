using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.API.DTOs.Customer
{
    public record CustomerUpdateRequestDTO(
        int Id,
        string FullName,
        string? Phone,
        string? Email
    );
}