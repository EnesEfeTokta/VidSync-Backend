using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Collections.Concurrent;
using VidSync.Domain.Entities;

namespace VidSync.Signaling.Hubs
{
    [Authorize]
    public class CommunicationHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> ConnectionToRoomMap = new();
        private static readonly ConcurrentDictionary<string, string> ConnectionToUserMap = new();
        private readonly UserManager<User> _userManager;

        public CommunicationHub(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task JoinRoom(string roomId)
        {
            var userId = Context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return;

            var connectionId = Context?.ConnectionId;
            
            ConnectionToRoomMap[connectionId ?? string.Empty] = roomId;
            ConnectionToUserMap[connectionId ?? string.Empty] = userId;
            await Groups.AddToGroupAsync(connectionId ?? string.Empty, roomId);

            Console.WriteLine($"User {userId} joined room {roomId}");

            await UpdateAndBroadcastParticipantList(roomId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            if (ConnectionToRoomMap.TryRemove(connectionId, out var roomId))
            {
                ConnectionToUserMap.TryRemove(connectionId, out _);

                Console.WriteLine($"A connection disconnected from room {roomId}");

                await UpdateAndBroadcastParticipantList(roomId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task UpdateAndBroadcastParticipantList(string roomId)
        {
            var userIdsInRoom = ConnectionToUserMap
                .Where(kvp => ConnectionToRoomMap.ContainsKey(kvp.Key) && ConnectionToRoomMap[kvp.Key] == roomId)
                .Select(kvp => kvp.Value)
                .Distinct()
                .ToList();

            var participants = new List<object>();
            foreach (var userId in userIdsInRoom)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    participants.Add(new { Id = user.Id.ToString(), FirstName = user.FirstName });
                }
            }
            
            await Clients.Group(roomId).SendAsync("UpdateParticipantList", participants);
        }
    }
}