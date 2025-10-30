using System.Net.Http.Json;
using VidSync.Domain.DTOs.AiSummaryChat.Request;
using VidSync.Domain.DTOs.AiSummaryChat.Response;
using VidSync.Domain.Interfaces;

namespace VidSync.Persistence.Services.AiService.Clients;

public class AiServiceClient : IAiServiceClient
{
    private readonly HttpClient _httpClient;

    public AiServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ConversationSummaryResponse> GetSummaryAsync(ConversationPayloadRequest payload)
    {
        var response = await _httpClient.PostAsJsonAsync("summarize", payload);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"AI service failed with status code {response.StatusCode}. Response: {errorContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<ConversationSummaryResponse>();
        return result ?? throw new HttpRequestException("AI service returned null response");
    }
}
