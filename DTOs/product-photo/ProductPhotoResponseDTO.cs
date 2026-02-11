using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.Api.DTOs.productphoto
{
   public record ProductPhotoResponseDTO(
        int Id,
        string PhotoUrl,
        bool IsPrimary
    );
}