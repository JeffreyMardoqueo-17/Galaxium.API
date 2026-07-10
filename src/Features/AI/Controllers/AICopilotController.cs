using Galaxium.Api.DTOs.AI;
using Galaxium.Api.Services.AI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Galaxium.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AICopilotController : ControllerBase
{
    private readonly IAICopilotService _aiCopilotService;
    private readonly ILogger<AICopilotController> _logger;

    public AICopilotController(
        IAICopilotService aiCopilotService,
        ILogger<AICopilotController> logger)
    {
        _aiCopilotService = aiCopilotService;
        _logger = logger;
    }

    [HttpPost("chat")]
    [ProducesResponseType(typeof(AIChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AIChatResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AIChatResponseDto>> Chat(
        [FromBody] AIChatRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AIChatResponseDto
            {
                Success = false,
                Error = "Solicitud inválida",
                Response = "Por favor verifica los datos enviados."
            });
        }

        var userId = GetUserId();
        var tenantId = GetTenantId();

        request.UserId = userId;
        request.TenantId = tenantId;

        _logger.LogInformation("AI Chat request from User:{UserId}, Tenant:{TenantId}",
            userId, tenantId);

        var response = await _aiCopilotService.ProcessMessageAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpGet("context/{conversationId}")]
    [ProducesResponseType(typeof(ConversationContextDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationContextDto>> GetContext(
        string conversationId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var context = await _aiCopilotService.GetConversationContextAsync(
            tenantId, userId, conversationId, cancellationToken);

        if (context == null)
        {
            return NotFound(new { message = "Conversación no encontrada" });
        }

        return Ok(context);
    }

    [HttpDelete("context/{conversationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearContext(
        string conversationId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        await _aiCopilotService.ClearConversationAsync(
            tenantId, userId, conversationId, cancellationToken);

        return NoContent();
    }

    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "AICopilot",
            timestamp = DateTime.UtcNow
        });
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (int.TryParse(userIdClaim, out var userId))
            return userId;

        return 1;
    }

    private int GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value
            ?? User.FindFirst("TenantId")?.Value;

        if (int.TryParse(tenantIdClaim, out var tenantId))
            return tenantId;

        return 1;
    }
}
