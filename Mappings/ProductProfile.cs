using AutoMapper;
using Galaxium.API.Entities;
using Galaxium.API.DTOs.Product;
using System;

namespace Galaxium.Api.Mappings
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            // ===============================
            // CREATE → ENTITY
            // ===============================
            CreateMap<ProductCreateRequestDTO, Product>()
                .ForMember(dest => dest.Stock,
                    opt => opt.MapFrom(src => src.InitialStock))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.SaleDetails, opt => opt.Ignore())
                .ForMember(dest => dest.StockMovements, opt => opt.Ignore());

            // ===============================
            // UPDATE → ENTITY
            // ===============================
            CreateMap<ProductUpdateRequestDTO, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Stock, opt => opt.Ignore())
                .ForMember(dest => dest.CostPrice, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.SaleDetails, opt => opt.Ignore())
                .ForMember(dest => dest.StockMovements, opt => opt.Ignore());

            // ===============================
            // ENTITY → RESPONSE (RECORD)
            // ===============================
            CreateMap<Product, ProductResponseDTO>()
                .ConstructUsing(src => new ProductResponseDTO(
                    src.Id,
                    src.Name,
                    src.SKU,
                    src.CostPrice,
                    src.SalePrice,
                    src.Stock,
                    src.MinimumStock,
                    src.IsActive,
                    src.CreatedAt,
                    src.CategoryId,
                    src.Category != null ? src.Category.Name : string.Empty,
                    src.CreatedByUserId,
                    src.CreatedByUser != null ? src.CreatedByUser.FullName : string.Empty
                ));
        }
    }
}
