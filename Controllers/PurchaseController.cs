using System.Security.Claims;
using Galaxium.Api.DTOs.Purchase;
using Galaxium.Api.Services.Interfaces;
using Galaxium.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxium.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = GalaxiumPolicyNames.InventoryManagement)]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchaseController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseResponseDto>> Create([FromBody] PurchaseCreateRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

        if (!int.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("Usuario invalido.");

        var purchase = await _purchaseService.CreateAsync(
            userId,
            request.SupplierId,
            request.Details.Select(d => (d.ProductId, d.Quantity, d.UnitPrice)));

        return CreatedAtAction(nameof(GetById), new { purchaseId = purchase.Id }, Map(purchase));
    }

    [HttpGet]
    [Authorize(Policy = GalaxiumPolicyNames.ReportsAccess)]
    public async Task<ActionResult<IEnumerable<PurchaseResponseDto>>> GetAll([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var purchases = await _purchaseService.GetAllAsync(startDate, endDate);
        return Ok(purchases.Select(Map));
    }

    [HttpGet("{purchaseId:int}")]
    [Authorize(Policy = GalaxiumPolicyNames.ReportsAccess)]
    public async Task<ActionResult<PurchaseResponseDto>> GetById(int purchaseId)
    {
        var purchase = await _purchaseService.GetByIdAsync(purchaseId);
        if (purchase == null)
            return NotFound();

        return Ok(Map(purchase));
    }

    private static PurchaseResponseDto Map(Galaxium.Api.Entities.Purchase purchase)
    {
        var details = purchase.Details
            .Select(d => new PurchaseDetailResponseDto(
                d.Id,
                d.ProductId,
                d.Product?.Name ?? string.Empty,
                d.Quantity,
                d.UnitPrice,
                d.Total))
            .ToList();

        return new PurchaseResponseDto(
            purchase.Id,
            purchase.SupplierId,
            purchase.Supplier?.Name ?? string.Empty,
            purchase.UserId,
            purchase.PurchaseDate,
            purchase.Total,
            purchase.Status,
            details
        );
    }
}
