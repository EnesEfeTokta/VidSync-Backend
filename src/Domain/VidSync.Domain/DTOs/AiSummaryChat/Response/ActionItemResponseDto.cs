using System.Text.Json.Serialization;

namespace VidSync.Domain.DTOs.AiSummaryChat.Response;

public class ActionItemResponseDto
{
    [JsonPropertyName("task_id")]
    public string TaskId { get; set; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;

    [JsonPropertyName("assigned_to_ids")]
    public List<string> AssignedToIds { get; set; } = null!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = null!;

    [JsonPropertyName("due_date")]
    public DateTime? DueDate { get; set; }

    [JsonPropertyName("source_message_id")]
    public int SourceMessageId { get; set; }
}
