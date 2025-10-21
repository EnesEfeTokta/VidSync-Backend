using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using VidSync.Domain.Entities;
using VidSync.Persistence;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace VidSync.Signaling.Hubs
{
    [Authorize]
    public class CommunicationHub : Hub
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IConnectionManager _connectionManager;
        private readonly IConnectionMultiplexer _redis;

        public CommunicationHub(
            UserManager<User> userManager,
            AppDbContext context,
            IConnectionManager connectionManager,
            IConnectionMultiplexer redis)
        {
            _userManager = userManager;
            _context = context;
            _connectionManager = connectionManager;
            _redis = redis;
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

            string ipAddress = GetIpAddress();
            var db = _redis.GetDatabase();
            var redisKey = $"active_users:{roomIdStr}";
            await db.SetAddAsync(redisKey, userId);

            var roomSession = await GetOrCreateActiveRoomSession(Guid.Parse(roomIdStr));

            var activity = new ParticipantActivity
            {
                Id = Guid.NewGuid(),
                RoomSessionId = roomSession.Id,
                UserId = currentUser.Id,
                IpAddress = ipAddress,
                JoinTime = DateTime.UtcNow
            };
            _context.ParticipantActivities.Add(activity);
            await _context.SaveChangesAsync();

            Context.Items["ParticipantActivityId"] = activity.Id;

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

            Console.WriteLine($"User {currentUser.UserName} and Ip: {ipAddress} joined room {roomIdStr}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var (userId, roomIdStr) = _connectionManager.RemoveConnection(Context.ConnectionId);

            if (userId != null && roomIdStr != null)
            {
                var db = _redis.GetDatabase();
                var redisKey = $"active_users:{roomIdStr}";

                await db.SetRemoveAsync(redisKey, userId);

                if (Context.Items.TryGetValue("ParticipantActivityId", out var activityIdObj) && activityIdObj is Guid activityId)
                {
                    var activity = await _context.ParticipantActivities.FindAsync(activityId);
                    if (activity != null && activity.LeaveTime == null)
                    {
                        activity.LeaveTime = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomIdStr);
                await Clients.OthersInGroup(roomIdStr).SendAsync("UserLeft", userId);
                Console.WriteLine($"User {userId} left room {roomIdStr}");
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

        private async Task<RoomSession> GetOrCreateActiveRoomSession(Guid roomId)
        {
            var roomSession = await _context.RoomSessions
                .FirstOrDefaultAsync(rs => rs.RoomId == roomId && rs.EndTime == null);

            if (roomSession == null)
            {
                roomSession = new RoomSession
                {
                    Id = Guid.NewGuid(),
                    RoomId = roomId,
                    StartTime = DateTime.UtcNow
                };
                _context.RoomSessions.Add(roomSession);
                await _context.SaveChangesAsync();
            }

            return roomSession;
        }

        private string GetIpAddress()
        {
            var httpContext = Context.GetHttpContext();
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            return ipAddress ?? "Unknown";
        }
    }
}