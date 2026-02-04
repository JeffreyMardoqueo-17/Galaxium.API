using AutoMapper;
using Galaxium.Api.DTOs.StockEntry;
using Galaxium.Api.Entities;
using System;

namespace Galaxium.Api.Mappings
{public class StockEntryProfile : Profile
{
    public StockEntryProfile()
    {
        // Entity → Response DTO (RECORD)
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
                src.IsActive,
                src.CreatedAt
            ));

        // Create DTO → Entity
        CreateMap<StockEntryCreateDTO, StockEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.TotalCost, opt => opt.Ignore()) // ✔ encapsulado
            .ForMember(dest => dest.CreatedAt,
                opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive,
                opt => opt.MapFrom(_ => true));
    }
}

}
