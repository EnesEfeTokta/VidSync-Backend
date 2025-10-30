using System.Text.Json.Serialization;

namespace VidSync.Domain.DTOs.AiSummaryChat.Response;

public class ProcessingResultsResponseDto
{
    [JsonPropertyName("summary")]
    public SummaryResponseDto Summary { get; set; } = null!;

    [JsonPropertyName("action_items")]
    public List<ActionItemResponseDto> ActionItems { get; set; } = null!;

    [JsonPropertyName("scheduled_events")]
    public List<ScheduledEventResponseDto> ScheduledEvents { get; set; } = null!;
}