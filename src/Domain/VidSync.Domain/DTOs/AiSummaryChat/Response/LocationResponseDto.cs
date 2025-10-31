using System.Text.Json.Serialization;

namespace VidSync.Domain.DTOs.AiSummaryChat.Response;

public class LocationResponseDto
{
    [JsonPropertyName("city")]
    public string City { get; set; } = null!;

    [JsonPropertyName("country")]
    public string Country { get; set; } = null!;
}
