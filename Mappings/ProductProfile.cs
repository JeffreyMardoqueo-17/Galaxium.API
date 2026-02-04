using AutoMapper;
using Galaxium.API.Entities;
using Galaxium.API.DTOs.Product;
using System;
using Galaxium.API.Models;

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
            // 🔒 Estado y negocio: NO en AutoMapper
            .ForMember(dest => dest.Stock, opt => opt.Ignore())
            .ForMember(dest => dest.CostPrice, opt => opt.Ignore())
            .ForMember(dest => dest.SalePrice, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())

            // 🔒 Claves y relaciones
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SKU, opt => opt.Ignore())
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
            .ForMember(dest => dest.SKU, opt => opt.Ignore())
            .ForMember(dest => dest.Stock, opt => opt.Ignore())
            .ForMember(dest => dest.CostPrice, opt => opt.Ignore())
            .ForMember(dest => dest.SalePrice, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.SaleDetails, opt => opt.Ignore())
            .ForMember(dest => dest.StockMovements, opt => opt.Ignore());


            // ===============================
            // ENTITY → RESPONSE
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
            // ===============================
            // FILTER DTO → FILTER MODEL
            // ===============================
            CreateMap<ProductFilterRequestDTO, ProductFilterModel>()
                .ForMember(dest => dest.OrderBy,
                    opt => opt.MapFrom(src =>
                        string.IsNullOrWhiteSpace(src.OrderBy)
                            ? "CreatedAt"
                            : src.OrderBy
                    ));
        }
    }
}