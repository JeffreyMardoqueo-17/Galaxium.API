using Galaxium.Api.Features.Tenants.Contracts.Requests;
using Galaxium.Api.Features.Tenants.Contracts.Responses;
using Galaxium.Api.Features.Tenants.Services;
using Galaxium.Api.Utils;
using Galaxium.API.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxium.Api.Features.Tenants.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = GalaxiumPolicyNames.AdminOnly)]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ITenantOnboardingService _onboardingService;

    public TenantController(
        ITenantService tenantService,
        ITenantOnboardingService onboardingService)
    {
        _tenantService = tenantService;
        _onboardingService = onboardingService;
    }

    [HttpPost("onboard")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TenantOnboardingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TenantOnboardingResponse>> Onboard(
        [FromBody] TenantOnboardingRequest request)
    {
        try
        {
            var result = await _onboardingService.OnboardAsync(request);
            return CreatedAtAction(
                nameof(GetById),
                new { tenantId = result.TenantId },
                result);
        }
        catch (BusinessException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantResponseDto>>> GetAll()
    {
        var tenants = await _tenantService.GetAllAsync();
        return Ok(tenants.Select(MapToDto));
    }

    [HttpGet("{tenantId:int}")]
    public async Task<ActionResult<TenantResponseDto>> GetById(int tenantId)
    {
        var tenant = await _tenantService.GetByIdAsync(tenantId);
        if (tenant == null) return NotFound();
        return Ok(MapToDto(tenant));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<TenantResponseDto>> Create([FromBody] TenantCreateRequest request)
    {
        try
        {
            var existing = await _tenantService.GetAllAsync();
            if (existing.Any())
                return Forbid();

            var tenant = await _tenantService.CreateAsync(
                request.Name,
                request.Slug,
                request.ContactEmail,
                request.PhoneNumber,
                request.Address,
                request.MaxUsers,
                request.MaxProducts);

            return CreatedAtAction(nameof(GetById), new { tenantId = tenant.Id }, MapToDto(tenant));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{tenantId:int}")]
    public async Task<ActionResult<TenantResponseDto>> Update(int tenantId, [FromBody] TenantUpdateRequest request)
    {
        try
        {
            var tenant = await _tenantService.UpdateAsync(
                tenantId,
                request.Name,
                request.Slug,
                request.ContactEmail,
                request.PhoneNumber,
                request.Address,
                request.LogoUrl,
                request.IsActive,
                request.MaxUsers,
                request.MaxProducts);

            if (tenant == null) return NotFound();
            return Ok(MapToDto(tenant));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private static TenantResponseDto MapToDto(API.Entities.Tenant t)
    {
        return new TenantResponseDto(
            t.Id,
            t.Name,
            t.Slug,
            t.ContactEmail,
            t.PhoneNumber,
            t.Address,
            t.LogoUrl,
            t.IsActive,
            t.CreatedAt,
            t.UpdatedAt,
            t.SubscriptionExpiresAt,
            t.MaxUsers,
            t.MaxProducts);
    }
}

public record ErrorResponse(string Message);
