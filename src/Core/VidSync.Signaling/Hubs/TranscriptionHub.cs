using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace VidSync.Signaling.Hubs;

[Authorize]
public class TranscriptionHub : Hub
{
    public async Task JoinSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        Console.WriteLine($"Client {Context.ConnectionId} joined transcription session {sessionId}");
    }

    public async Task StreamAudio(IAsyncEnumerable<byte[]> stream, string sessionId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine($"User {userId} started streaming audio for session {sessionId}");

        try
        {
            await foreach (var audioChunk in stream)
            {
                Console.WriteLine($"Received audio chunk of size {audioChunk.Length} from user {userId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during audio streaming for user {userId}: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"User {userId} stopped streaming audio for session {sessionId}");
        }
    }

    public async Task SendTranscriptionResult(string sessionId, string transcript, bool isFinal)
    {
        await Clients.Group(sessionId).SendAsync("ReceiveTranscript", transcript, isFinal);
    }
}
