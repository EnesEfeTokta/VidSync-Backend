using System.Text.Json.Serialization;

namespace VidSync.Domain.DTOs.AiSummaryChat.Response;

public class ConversationSummaryResponse
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = null!;

    [JsonPropertyName("participants")]
    public List<ParticipantResponseDto> Participants { get; set; } = new();
}
