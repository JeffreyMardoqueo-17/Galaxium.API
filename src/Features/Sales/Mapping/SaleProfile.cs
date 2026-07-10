using System;
using AutoMapper;
using Galaxium.API.Entities;
using Galaxium.API.DTOs;

namespace Galaxium.Api.Mappings
{
    public class SaleProfile : Profile
    {
        private static readonly TimeZoneInfo ElSalvadorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");

        public SaleProfile()
        {
            CreateMap<Sale, SaleResponseDto>()
                .ForMember(dest => dest.SaleDate, opt => opt.MapFrom(src => ToLocalTime(src.SaleDate)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => ToLocalTime(src.CreatedAt)))
                .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount))
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details));

            CreateMap<SaleCreateDto, Sale>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SubTotal, opt => opt.Ignore())
                .ForMember(dest => dest.Total, opt => opt.Ignore())
                .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid ?? 0))
                .ForMember(dest => dest.ChangeAmount, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "COMPLETED"))
                .ForMember(dest => dest.InvoiceNumber, opt => opt.Ignore())
                .ForMember(dest => dest.SaleDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsDiscountPercentage, opt => opt.MapFrom(src => src.IsDiscountPercentage))
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore())
                .ForMember(dest => dest.Details, opt => opt.Ignore());

            CreateMap<SaleDetail, SaleDetailResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

            CreateMap<SaleDetailCreateDto, SaleDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.UnitCost, opt => opt.Ignore())
                .ForMember(dest => dest.SubTotal, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Sale, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());
        }

        private static DateTime ToLocalTime(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, ElSalvadorTimeZone);
            }
            return dateTime;
        }
    }
}
