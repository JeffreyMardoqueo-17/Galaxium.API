using AutoMapper;
using Galaxium.API.Entities;
using Galaxium.API.DTOs.Customer;
using System;

namespace Galaxium.Api.Mappings
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            // CREATE → ENTITY
            CreateMap<CustomerCreateRequestDTO, Customer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Sales, opt => opt.Ignore());

            // UPDATE → ENTITY
            CreateMap<CustomerUpdateRequestDTO, Customer>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Sales, opt => opt.Ignore());

            // ENTITY → RESPONSE
            CreateMap<Customer, CustomerResponseDTO>()
                .ConstructUsing(src => new CustomerResponseDTO(
                    src.Id,
                    src.FullName,
                    src.Phone,
                    src.Email,
                    src.CreatedAt
                ));
        }
    }
}
