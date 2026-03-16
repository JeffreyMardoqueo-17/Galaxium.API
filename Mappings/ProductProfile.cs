using AutoMapper;
using Galaxium.API.Entities;
using Galaxium.API.DTOs.Product;
using System;
using Galaxium.API.Models;
using Galaxium.Api.DTOs.Product;
using Galaxium.Api.DTOs.productphoto;
using Galaxium.Api.Enums;

namespace Galaxium.Api.Mappings
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<ProductPhoto, ProductPhotoResponseDTO>();

            CreateMap<Product, ProductWithPhotosResponseDTO>()
                .ForMember(dest => dest.Photos,
                    opt => opt.MapFrom(src => src.Photos));
            CreateMap<Product, ProductWithPhotosResponseDTO>()
    .ConstructUsing((src, ctx) => new ProductWithPhotosResponseDTO(
        src.Id,
        src.Name,
        src.SKU,
        src.Barcode,
        src.CostPrice,
        src.SalePrice,
        src.Stock,
        src.MinimumStock,
        src.UnitOfMeasure.ToString(),
        src.IsActive,
        src.CreatedAt,
        src.CategoryId,
        src.Category != null ? src.Category.Name : string.Empty,
        src.CreatedByUserId,
        src.CreatedByUser != null ? src.CreatedByUser.FullName : string.Empty,
        ctx.Mapper.Map<IEnumerable<ProductPhotoResponseDTO>>(src.Photos)
    ));


            // ===============================
            // CREATE → ENTITY
            // ===============================
            CreateMap<ProductCreateRequestDTO, Product>()
            .ForMember(dest => dest.UnitOfMeasure,
                opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.UnitOfMeasure)
                        ? UnitOfMeasure.Unit
                        : Enum.Parse<UnitOfMeasure>(src.UnitOfMeasure, true)))
            .ForMember(dest => dest.Stock, opt => opt.Ignore())
            .ForMember(dest => dest.CostPrice, opt => opt.Ignore())
            .ForMember(dest => dest.SalePrice, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())

            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SKU, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.SaleDetails, opt => opt.Ignore());



            // ===============================
            // UPDATE → ENTITY
            // ===============================
            CreateMap<ProductUpdateRequestDTO, Product>()
            .ForMember(dest => dest.UnitOfMeasure,
            opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.UnitOfMeasure)
                    ? UnitOfMeasure.Unit
                    : Enum.Parse<UnitOfMeasure>(src.UnitOfMeasure, true)))
         .ForMember(dest => dest.Id, opt => opt.Ignore())
         .ForMember(dest => dest.SKU, opt => opt.Ignore())
         .ForMember(dest => dest.Stock, opt => opt.Ignore())
         .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
         .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
         .ForMember(dest => dest.Category, opt => opt.Ignore())
         .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
         .ForMember(dest => dest.SaleDetails, opt => opt.Ignore());


            // ===============================
            // ENTITY → RESPONSE
            // ===============================
            CreateMap<Product, ProductResponseDTO>()
     .ConstructUsing(src => new ProductResponseDTO(
         src.Id,
         src.Name,
         src.SKU,
         src.Barcode!,   // 👈 NUEVO

         src.CostPrice,
         src.SalePrice,
         src.Stock,
         src.MinimumStock,
         src.UnitOfMeasure.ToString(),
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