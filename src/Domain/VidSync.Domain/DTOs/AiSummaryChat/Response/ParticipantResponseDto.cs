using System.Text.Json.Serialization;

namespace VidSync.Domain.DTOs.AiSummaryChat.Response;

public class ParticipantResponseDto
{
    [JsonPropertyName("participant_id")]
    public string ParticipantId { get; set; } = null!;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = null!;

    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [JsonPropertyName("role")]
    public string Role { get; set; } = null!;

    [JsonPropertyName("is_moderator")]
    public bool IsModerator { get; set; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = null!;

    [JsonPropertyName("utc_offset")]
    public string UtcOffset { get; set; } = null!;

    [JsonPropertyName("device")]
    public string Device { get; set; } = null!;

    [JsonPropertyName("location")]
    public LocationResponseDto Location { get; set; } = null!;
}
