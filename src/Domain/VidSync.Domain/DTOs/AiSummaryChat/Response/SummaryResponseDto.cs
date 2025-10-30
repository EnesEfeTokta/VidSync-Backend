using System.Text.Json.Serialization;

namespace VidSync.Domain.DTOs.AiSummaryChat.Response;

public class SummaryResponseDto
{
    [JsonPropertyName("overall")]
    public string Overall { get; set; } = null!;

    [JsonPropertyName("key_points")]
    public List<string> KeyPoints { get; set; } = null!;
}