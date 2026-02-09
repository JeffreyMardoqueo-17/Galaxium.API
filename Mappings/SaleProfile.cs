using System;
using AutoMapper;
using Galaxium.API.Entities;
using Galaxium.API.DTOs;

namespace Galaxium.Api.Mappings
{
    public class SaleProfile : Profile
    {
        public SaleProfile()
        {
            // ================================
            // SALE ENTITY → RESPONSE DTO
            // ================================
            CreateMap<Sale, SaleResponseDto>()
                .ConstructUsing(src => new SaleResponseDto(
                    src.Id,
                    src.CustomerId,
                    src.UserId,
                    src.PaymentMethodId,
                    src.SubTotal,
                    src.Discount,
                    src.Total,
                    src.AmountPaid,
                    src.ChangeAmount,
                    src.Status,
                    src.InvoiceNumber,
                    src.SaleDate,
                    src.CreatedAt,
                    src.Details != null
                        ? src.Details.Select(d => new SaleDetailResponseDto(
                            d.Id,
                            d.SaleId,
                            d.ProductId,
                            d.Quantity,
                            d.UnitPrice,
                            d.UnitCost,
                            d.SubTotal,
                            d.CreatedAt,
                            d.Product != null ? d.Product.Name : string.Empty
                        )).ToList()
                        : new List<SaleDetailResponseDto>()
                ));

            // ================================
            // SALE CREATE DTO → ENTITY
            // ================================
            CreateMap<SaleCreateDto, Sale>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SubTotal, opt => opt.Ignore())
                .ForMember(dest => dest.Total, opt => opt.Ignore())
                .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid ?? 0))
                .ForMember(dest => dest.ChangeAmount, opt => opt.Ignore()) // Calculado en el servicio
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "COMPLETED"))
                .ForMember(dest => dest.InvoiceNumber, opt => opt.Ignore())
                .ForMember(dest => dest.SaleDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore())
                // ⚠️ CRÍTICO: NO mapear Details aquí porque se mapean por separado en el controlador
                .ForMember(dest => dest.Details, opt => opt.Ignore());

            // ================================
            // SALE DETAIL ENTITY → RESPONSE DTO
            // ================================
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

            // ================================
            // SALE DETAIL CREATE DTO → ENTITY
            // ================================
            CreateMap<SaleDetailCreateDto, SaleDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.UnitCost, opt => opt.Ignore())
                .ForMember(dest => dest.SubTotal, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Sale, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());
        }
    }
}
