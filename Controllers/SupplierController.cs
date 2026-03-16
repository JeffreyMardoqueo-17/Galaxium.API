using Galaxium.Api.DTOs.Supplier;
using Galaxium.Api.Entities;
using Galaxium.Api.Services.Interfaces;
using Galaxium.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxium.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = GalaxiumPolicyNames.InventoryManagement)]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SupplierController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierResponseDto>>> GetAll()
    {
        var data = await _supplierService.GetAllAsync();
        return Ok(data.Select(Map));
    }

    [HttpGet("{supplierId:int}")]
    public async Task<ActionResult<SupplierResponseDto>> GetById(int supplierId)
    {
        var supplier = await _supplierService.GetByIdAsync(supplierId);
        if (supplier == null)
            return NotFound();

        return Ok(Map(supplier));
    }

    [HttpPost]
    public async Task<ActionResult<SupplierResponseDto>> Create([FromBody] SupplierCreateRequestDto request)
    {
        var created = await _supplierService.AddAsync(new Supplier
        {
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
        });

        return CreatedAtAction(nameof(GetById), new { supplierId = created.Id }, Map(created));
    }

    [HttpPut("{supplierId:int}")]
    public async Task<ActionResult<SupplierResponseDto>> Update(int supplierId, [FromBody] SupplierUpdateRequestDto request)
    {
        var updated = await _supplierService.UpdateAsync(supplierId, new Supplier
        {
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            IsActive = request.IsActive,
        });

        return Ok(Map(updated));
    }

    private static SupplierResponseDto Map(Supplier supplier)
    {
        return new SupplierResponseDto(
            supplier.Id,
            supplier.Name,
            supplier.Phone,
            supplier.Email,
            supplier.Address,
            supplier.IsActive,
            supplier.CreatedAt
        );
    }
}
