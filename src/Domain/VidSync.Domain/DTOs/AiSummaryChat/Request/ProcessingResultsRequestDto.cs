using System.Text.Json.Serialization;

namespace VidSync.Domain.DTOs.AiSummaryChat.Request;

public class ProcessingResultsRequestDto
{
    [JsonPropertyName("summary")]
    public object Summary { get; set; } = new { key_points = new List<string>(), overall = (string?)null };

    [JsonPropertyName("action_items")]
    public List<object> ActionItems { get; set; } = new List<object>();

    [JsonPropertyName("scheduled_events")]
    public List<object> ScheduledEvents { get; set; } = new List<object>();
}