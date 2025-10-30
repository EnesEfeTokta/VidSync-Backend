namespace VidSync.Domain.Interfaces;

public interface IConversationSummarizationService
{
    Task SummarizeAndSaveAsync(Guid roomId);
}
