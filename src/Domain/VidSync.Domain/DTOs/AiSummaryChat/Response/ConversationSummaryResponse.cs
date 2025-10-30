using System.Text.Json.Serialization;
using VidSync.Domain.DTOs.AiSummaryChat.Request;

namespace VidSync.Domain.DTOs.AiSummaryChat.Response;

public class ConversationSummaryResponse
{
    [JsonPropertyName("metadata")]
    public MetadataResponseDto Metadata { get; set; } = null!;

    [JsonPropertyName("participants")]
    public List<ParticipantRequestDto> Participants { get; set; } = new List<ParticipantRequestDto>();

    [JsonPropertyName("chat_history")]
    public List<ChatMessageRequestDto> ChatHistory { get; set; } = new List<ChatMessageRequestDto>();

    [JsonPropertyName("processing_results")]
    public ProcessingResultsResponseDto ProcessingResults { get; set; } = null!;
}
