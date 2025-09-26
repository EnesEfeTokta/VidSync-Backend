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

            var currentUser = await _userManager.FindByIdAsync(userId);
            if (currentUser == null) return;

            var connectionId = Context?.ConnectionId;

            var otherUserIdsInRoom = ConnectionToUserMap
                .Where(kvp => kvp.Key != connectionId && ConnectionToRoomMap.ContainsKey(kvp.Key) && ConnectionToRoomMap[kvp.Key] == roomId)
                .Select(kvp => kvp.Value).Distinct().ToList();

            var existingParticipants = new List<object>();
            foreach (var otherUserId in otherUserIdsInRoom)
            {
                var user = await _userManager.FindByIdAsync(otherUserId);
                if (user != null)
                {
                    existingParticipants.Add(new { Id = user.Id.ToString(), FirstName = user.FirstName });
                }
            }

            await Clients.Caller.SendAsync("ExistingParticipants", existingParticipants);

            await Groups.AddToGroupAsync(connectionId ?? string.Empty, roomId);
            ConnectionToRoomMap[connectionId ?? string.Empty] = roomId;
            ConnectionToUserMap[connectionId ?? string.Empty] = userId;

            await Clients.OthersInGroup(roomId).SendAsync("UserJoined", new
            {
                Id = currentUser.Id.ToString(),
                FirstName = currentUser.FirstName
            });

            Console.WriteLine($"User {currentUser.UserName} joined room {roomId}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            if (ConnectionToRoomMap.TryRemove(connectionId, out var roomId))
            {
                if (ConnectionToUserMap.TryRemove(connectionId, out var userId))
                {
                    await Clients.OthersInGroup(roomId).SendAsync("UserLeft", userId);
                    Console.WriteLine($"User {userId} left room {roomId}");
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendOffer(string targetuserId, object offer)
        {
            var callingUserId = Context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(callingUserId)) return;

            var targetConnectionId = ConnectionToUserMap.FirstOrDefault(kvp => kvp.Value == targetuserId).Key;
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", offer);
                Console.WriteLine($"Offer sent from {callingUserId} to {targetuserId}");
            }
        }

        public async Task SendAnswer(string targetuserId, object answer)
        {
            var callingUserId = Context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(callingUserId)) return;

            var targetConnectionId = ConnectionToUserMap.FirstOrDefault(kvp => kvp.Value == targetuserId).Key;
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", answer);
                Console.WriteLine($"Answer sent from {callingUserId} to {targetuserId}");
            }
        }

        public async Task SendIceCandidate(string targetuserId, object candidate)
        {
            var callingUserId = Context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(callingUserId)) return;

            var targetConnectionId = ConnectionToUserMap.FirstOrDefault(kvp => kvp.Value == targetuserId).Key;
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", candidate);
                Console.WriteLine($"ICE candidate sent from {callingUserId} to {targetuserId}");
            }
        }
    }
}