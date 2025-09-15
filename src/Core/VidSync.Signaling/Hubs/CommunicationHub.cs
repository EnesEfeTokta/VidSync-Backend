using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using VidSync.Signaling.Helpers;

namespace VidSync.Signaling.Hubs;

[Authorize]
public class CommunicationHub : Hub
{
    private static readonly ConnectionMapping<string> _connections = new ConnectionMapping<string>();

    public async Task JoinRoom(string roomId)
    {
        var userId = Context.UserIdentifier;

        _connections.Add(userId ?? string.Empty, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.OthersInGroup(roomId).SendAsync("UserJoined", userId);
    }

    public override async Task OnDisconnectedAsync(System.Exception? exception)
    {
            var userId = Context.UserIdentifier;
            
            if (_connections.TryGetRoom(Context.ConnectionId, out var roomId))
            {
                _connections.Remove(Context.ConnectionId);
                await Clients.OthersInGroup(roomId ?? string.Empty).SendAsync("UserLeft", userId);
            }
            
            await base.OnDisconnectedAsync(exception);
    }
}