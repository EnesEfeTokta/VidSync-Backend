using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Collections.Concurrent;
using VidSync.Domain.Entities;
using VidSync.Persistence;
using Microsoft.EntityFrameworkCore;

namespace VidSync.Signaling.Hubs
{
    [Authorize]
    public class CommunicationHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> ConnectionToRoomMap = new();
        private static readonly ConcurrentDictionary<string, string> ConnectionToUserMap = new();
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public CommunicationHub(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task JoinRoom(string roomId)
        {
            var userId = Context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("JoinRoom failed: UserId is null or empty");
                throw new HubException("UserId is null or empty");
            }

            var currentUser = await _userManager.FindByIdAsync(userId);
            if (currentUser == null)
            {
                Console.WriteLine($"JoinRoom failed: User not found for UserId: {userId}");
                throw new HubException($"User not found for UserId: {userId}");
            }

            var connectionId = Context?.ConnectionId;
            if (string.IsNullOrEmpty(connectionId))
            {
                Console.WriteLine("JoinRoom failed: ConnectionId is null or empty");
                throw new HubException("ConnectionId is null or empty");
            }

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

            await Groups.AddToGroupAsync(connectionId, roomId);
            ConnectionToRoomMap[connectionId] = roomId;
            ConnectionToUserMap[connectionId] = userId;

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
                    await Groups.RemoveFromGroupAsync(connectionId, roomId);
                    Console.WriteLine($"User {userId} left room {roomId}");
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendOffer(string targetUserId, string serializedOffer)
        {
            var callingUserId = Context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(callingUserId))
            {
                Console.WriteLine("SendOffer failed: CallingUserId is null or empty");
                throw new HubException("CallingUserId is null or empty");
            }

            if (string.IsNullOrEmpty(targetUserId) || string.IsNullOrEmpty(serializedOffer))
            {
                Console.WriteLine($"SendOffer failed: Invalid parameters - targetUserId: {targetUserId}, serializedOffer: {serializedOffer}");
                throw new HubException($"Invalid parameters - targetUserId: {targetUserId}, serializedOffer: {serializedOffer}");
            }

            var targetConnectionId = ConnectionToUserMap.FirstOrDefault(kvp => kvp.Value == targetUserId).Key;
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", callingUserId, serializedOffer);
                Console.WriteLine($"Offer sent from {callingUserId} to {targetUserId}: {serializedOffer}");
            }
            else
            {
                Console.WriteLine($"SendOffer failed: No connection found for targetUserId: {targetUserId}");
                throw new HubException($"No connection found for targetUserId: {targetUserId}");
            }
        }

        public async Task SendAnswer(string targetUserId, string serializedAnswer)
        {
            var callingUserId = Context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(callingUserId))
            {
                Console.WriteLine("SendAnswer failed: CallingUserId is null or empty");
                throw new HubException("CallingUserId is null or empty");
            }

            if (string.IsNullOrEmpty(targetUserId) || string.IsNullOrEmpty(serializedAnswer))
            {
                Console.WriteLine($"SendAnswer failed: Invalid parameters - targetUserId: {targetUserId}, serializedAnswer: {serializedAnswer}");
                throw new HubException($"Invalid parameters - targetUserId: {targetUserId}, serializedAnswer: {serializedAnswer}");
            }

            var targetConnectionId = ConnectionToUserMap.FirstOrDefault(kvp => kvp.Value == targetUserId).Key;
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", serializedAnswer);
                Console.WriteLine($"Answer sent from {callingUserId} to {targetUserId}: {serializedAnswer}");
            }
            else
            {
                Console.WriteLine($"SendAnswer failed: No connection found for targetUserId: {targetUserId}");
                throw new HubException($"No connection found for targetUserId: {targetUserId}");
            }
        }

        public async Task SendIceCandidate(string targetUserId, string candidate)
        {
            var callingUserId = Context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(callingUserId))
            {
                Console.WriteLine("SendIceCandidate failed: CallingUserId is null or empty");
                throw new HubException("CallingUserId is null or empty");
            }

            if (string.IsNullOrEmpty(targetUserId) || string.IsNullOrEmpty(candidate))
            {
                Console.WriteLine($"SendIceCandidate failed: Invalid parameters - targetUserId: {targetUserId}, candidate: {candidate}");
                throw new HubException($"Invalid parameters - targetUserId: {targetUserId}, candidate: {candidate}");
            }

            try
            {
                System.Text.Json.JsonDocument.Parse(candidate);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine($"SendIceCandidate failed: Invalid candidate JSON - {ex.Message}");
                throw new HubException($"Invalid candidate JSON: {ex.Message}");
            }

            var targetConnectionId = ConnectionToUserMap.FirstOrDefault(kvp => kvp.Value == targetUserId).Key;
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", candidate);
                Console.WriteLine($"ICE candidate sent from {callingUserId} to {targetUserId}: {candidate}");
            }
            else
            {
                Console.WriteLine($"SendIceCandidate failed: No connection found for targetUserId: {targetUserId}");
                throw new HubException($"No connection found for targetUserId: {targetUserId}");
            }
        }

        public async Task SendMessage(string messageContent)
        {
            if (!Guid.TryParse(Context.UserIdentifier, out var senderId))
            {
                return;
            }

            if (!ConnectionToRoomMap.TryGetValue(Context.ConnectionId, out var roomId))
            {
                return;
            }

            var message = new Message
            {
                Id = Guid.NewGuid(),
                RoomId = Guid.Parse(roomId),
                SenderId = senderId,
                Content = messageContent,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var sender = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == senderId);
            if (sender == null)
            {
                return;
            }

            string senderName = sender != null ? $"{sender.FirstName} {sender.MiddleName} {sender.LastName}" : "Unknown";

            var messageDto = new
            {
                Id = message.Id,
                RoomId = message.RoomId,
                SenderId = message.SenderId,
                Content = message.Content,
                SentAt = message.SentAt,
                SenderName = senderName
            };

            await Clients.Group(roomId).SendAsync("ReceiveMessage", messageDto);
        }
    }
}