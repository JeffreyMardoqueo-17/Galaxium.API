using AutoMapper;
using Galaxium.API.Entities;
using Galaxium.Api.DTOs.ProductCategory;
using System;

namespace Galaxium.Api.Mappings
{
    public class ProductCategoryProfile : Profile
    {
        public ProductCategoryProfile()
        {
            // Map from Entity to Read DTO
            CreateMap<ProductCategory, ProductCategoryReadDto>();

            // Map from Request DTO to Entity
            CreateMap<ProductCategoryRequestDTO, ProductCategory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // para creación no modificar Id
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}


// namespace Galaxium.Api.DTOs.ProductCategory
// {
//     public record ProductCategoryReadDto
//     {
//         int Id;
//         string Name;
//         DateTime CreatedAt;
//     }
// }
    // public record ProductCategoryRequestDTO
    // {
    //     string Name;
    // }