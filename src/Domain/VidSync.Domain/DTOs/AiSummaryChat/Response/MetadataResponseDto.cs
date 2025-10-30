using System.Text.Json.Serialization;

namespace VidSync.Domain.DTOs.AiSummaryChat.Response;

public class MetadataResponseDto
{
    [JsonPropertyName("chat_id")]
    public string ChatId { get; set; } = null!;

    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("start_time_utc")]
    public DateTime StartTimeUtc { get; set; }

    [JsonPropertyName("end_time_utc")]
    public DateTime EndTimeUtc { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = null!;
        
    [JsonPropertyName("processing_status")]
    public string ProcessingStatus { get; set; } = null!;

    [JsonPropertyName("processing_timestamp_utc")]
    public DateTime ProcessingTimestampUtc { get; set; }
}
