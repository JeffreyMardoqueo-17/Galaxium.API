using AutoMapper;
using Galaxium.Api.DTOs.PaymentMethod;
using Galaxium.Api.Entities;

namespace Galaxium.Api.Mappings
{
    public class PaymentMethodProfile : Profile
    {
        public PaymentMethodProfile()
        {
            // ENTITY → RESPONSE DTO
            CreateMap<PaymentMethod, PaymentMethodResponseDto>()
                .ConstructUsing(src => new PaymentMethodResponseDto(
                    src.Id,
                    src.Name,
                    src.Description,
                    src.IsActive,
                    src.CreatedAt
                ));
        }
    }
}
