using AutoMapper;
using Galaxium.Api.DTOs.StockEntry;
using Galaxium.Api.Entities;

namespace Galaxium.Api.Mappings
{
    public class StockEntryProfile : Profile
    {
        public StockEntryProfile()
        {  
            // ENTITY → RESPONSE
            CreateMap<StockEntry, StockEntryResponseDTO>()
                .ConstructUsing(src => new StockEntryResponseDTO(
                    src.Id,
                    src.ProductId,
                    src.Product.Name,
                    src.UserId,
                    src.User.FullName,
                    src.Quantity,
                    src.UnitCost,
                    src.TotalCost,
                    src.ReferenceType.ToString(),
                    src.ReferenceId,
                    src.CreatedAt
                ));

            // CREATE DTO → ENTITY
            CreateMap<StockEntryCreateDTO, StockEntry>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}
