using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.Api.DTOs.PaymentMethod
{
    public record PaymentMethodResponseDto(
        int Id,
        string Name,
        string? Description,
        bool IsActive,
        DateTime CreatedAt
    );
}