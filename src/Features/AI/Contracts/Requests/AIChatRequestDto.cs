using System.ComponentModel.DataAnnotations;

namespace Galaxium.Api.DTOs.AI;

public class AIChatRequestDto
{
    [Required]
    [MinLength(1)]
    public string Message { get; set; } = string.Empty;

    public string? ConversationId { get; set; }

    [Required]
    public int TenantId { get; set; }

    [Required]
    public int UserId { get; set; }
}
