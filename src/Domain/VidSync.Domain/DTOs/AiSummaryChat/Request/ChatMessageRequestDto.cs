using System.Text.Json.Serialization;

namespace VidSync.Domain.DTOs.AiSummaryChat.Request;

public class ChatMessageRequestDto
{
    [JsonPropertyName("message_id")]
    public int MessageId { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = null!;

    [JsonPropertyName("sender_id")]
    public string SenderId { get; set; } = null!;

    [JsonPropertyName("message")]
    public string Message { get; set; } = null!;
}