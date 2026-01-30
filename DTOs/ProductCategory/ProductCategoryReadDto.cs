using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.Api.DTOs.ProductCategory
{
    public record ProductCategoryReadDto
    (int Id, string Name, DateTime CreatedAt);
}