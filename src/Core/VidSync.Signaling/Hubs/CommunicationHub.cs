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
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IConnectionManager _connectionManager;

        public CommunicationHub(UserManager<User> userManager, AppDbContext context, IConnectionManager connectionManager)
        {
            _userManager = userManager;
            _context = context;
            _connectionManager = connectionManager;
        }

        public async Task JoinRoom(Guid roomId)
        {
            string roomIdStr = roomId.ToString();
            if (string.IsNullOrEmpty(roomIdStr))
            {
                Console.WriteLine("JoinRoom failed: roomId is null or empty");
                throw new HubException("roomId is null or empty");
            }

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

            var otherUserIdsInRoom = await _connectionManager.GetUsersInRoomAsync(roomId, connectionId);

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

            await Groups.AddToGroupAsync(connectionId, roomIdStr);
            _connectionManager.AddConnection(connectionId, userId, roomId);

            await Clients.OthersInGroup(roomIdStr).SendAsync("UserJoined", new
            {
                Id = currentUser.Id.ToString(),
                FirstName = currentUser.FirstName
            });

            Console.WriteLine($"User {currentUser.UserName} joined room {roomIdStr}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var (userId, roomId) = _connectionManager.RemoveConnection(Context.ConnectionId);

            if (userId != null && roomId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
                await Clients.OthersInGroup(roomId).SendAsync("UserLeft", userId);
                Console.WriteLine($"User {userId} left room {roomId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        private async Task SendSignalToUser(string targetUserId, string messageType, params object[] payload)
        {
            var callingUserId = Context.UserIdentifier!;
            var targetConnectionId = await _connectionManager.GetConnectionIdAsync(targetUserId);

            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                var allArgs = new List<object> { callingUserId };
                allArgs.AddRange(payload);
                
                await Clients.Client(targetConnectionId).SendCoreAsync(messageType, payload);
                Console.WriteLine($"{messageType} sent from {callingUserId} to {targetUserId}");
            }
            else
            {
                Console.WriteLine($"Send failed: No connection found for targetUserId: {targetUserId}");
            }
        }
        
        public async Task SendOffer(string targetUserId, string serializedOffer)
        {
            var callerId = Context.UserIdentifier!;
            var targetConnectionId = await _connectionManager.GetConnectionIdAsync(targetUserId);
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", callerId, serializedOffer);
            }
        }

        public async Task SendAnswer(string targetUserId, string serializedAnswer)
        {
            var targetConnectionId = await _connectionManager.GetConnectionIdAsync(targetUserId);
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", serializedAnswer);
            }
        }

        public async Task SendIceCandidate(string targetUserId, string candidate)
        {
            var targetConnectionId = await _connectionManager.GetConnectionIdAsync(targetUserId);
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", candidate);
            }
        }

        public async Task SendMessage(string messageContent)
        {
            var senderId = Guid.Parse(Context.UserIdentifier!);
            var roomIdString = await _connectionManager.GetRoomForConnectionAsync(Context.ConnectionId);

            if (string.IsNullOrEmpty(roomIdString) || !Guid.TryParse(roomIdString, out var roomId))
            {
                return;
            }

            var message = new Message
            {
                Id = Guid.NewGuid(),
                RoomId = Guid.Parse(roomIdString),
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

            await Clients.Group(roomIdString).SendAsync("ReceiveMessage", messageDto);
        }
    }
}