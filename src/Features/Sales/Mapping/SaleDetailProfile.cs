using System;
using AutoMapper;
using Galaxium.API.Entities;
using Galaxium.API.DTOs;

namespace Galaxium.Api.Mappings
{
    public class SaleDetailProfile : Profile
    {
        public SaleDetailProfile()
        {
            // ========================================
            // ENTITY → RESPONSE DTO
            // ========================================
            CreateMap<SaleDetail, SaleDetailResponseDto>()
                .ConstructUsing(src => new SaleDetailResponseDto(
                    src.Id,
                    src.SaleId,
                    src.ProductId,
                    src.Quantity,
                    src.UnitPrice,
                    src.UnitCost,
                    src.SubTotal,
                    src.CreatedAt,
                    src.Product != null ? src.Product.Name : string.Empty
                ));

            // ========================================
            // CREATE DTO → ENTITY
            // ========================================
            CreateMap<SaleDetailCreateDto, SaleDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())           // Se genera en DB
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())   // Se calcula luego
                .ForMember(dest => dest.UnitCost, opt => opt.Ignore())    // Se calcula luego
                .ForMember(dest => dest.SubTotal, opt => opt.Ignore())    // Se calcula luego
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Sale, opt => opt.Ignore())       // Se asigna manualmente
                .ForMember(dest => dest.Product, opt => opt.Ignore());   // Se asigna manualmente
        }
    }
}
