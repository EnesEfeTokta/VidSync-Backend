using System.Text.Json.Serialization;

namespace VidSync.Domain.DTOs.AiSummaryChat.Request;

public class ParticipantRequestDto
{
    [JsonPropertyName("participant_id")]
    public string ParticipantId { get; set; } = null!;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = null!;

    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [JsonPropertyName("role")]
    public string Role { get; set; } = null!;
}
