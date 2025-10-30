using System.Text.Json.Serialization;

namespace VidSync.Domain.DTOs.AiSummaryChat.Request;

public class ConversationPayloadRequest
{
    [JsonPropertyName("metadata")]
    public MetadataRequestDto Metadata { get; set; } = null!;

    [JsonPropertyName("participants")]
    public List<ParticipantRequestDto> Participants { get; set; } = new List<ParticipantRequestDto>();

    [JsonPropertyName("chat_history")]
    public List<ChatMessageRequestDto> ChatHistory { get; set; } = new List<ChatMessageRequestDto>();

    [JsonPropertyName("processing_results")]
    public ProcessingResultsRequestDto ProcessingResults { get; set; } = null!;
}
