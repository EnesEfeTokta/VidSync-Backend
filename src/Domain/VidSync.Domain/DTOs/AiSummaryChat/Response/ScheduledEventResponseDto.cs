using System.Text.Json.Serialization;

namespace VidSync.Domain.DTOs.AiSummaryChat.Response;

public class ScheduledEventResponseDto
{
    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = null!;

    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("start_time_utc")]
    public DateTime StartTimeUtc { get; set; }

    [JsonPropertyName("duration_minutes")]
    public int DurationMinutes { get; set; }

    [JsonPropertyName("attendee_ids")]
    public List<string> AttendeeIds { get; set; } = new List<string>();

    [JsonPropertyName("status")]
    public string Status { get; set; } = null!;

    [JsonPropertyName("source_message_id")]
    public int SourceMessageId { get; set; }
}