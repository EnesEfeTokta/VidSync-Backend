using VidSync.Domain.DTOs.AiSummaryChat.Request;
using VidSync.Domain.DTOs.AiSummaryChat.Response;

namespace VidSync.Domain.Interfaces;

public interface IAiServiceClient
{
    Task<ConversationSummaryResponse> GetSummaryAsync(ConversationPayloadRequest payload);
}
