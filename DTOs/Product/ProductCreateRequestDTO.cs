using System;

namespace Galaxium.API.DTOs.Product
{
  public record ProductCreateRequestDTO(
      int CategoryId,
      string Name,
      int MinimumStock
  );



}