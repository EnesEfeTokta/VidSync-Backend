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

        public async Task joinRoom(Guid roomId)
        {
            string roomIdStr = roomId.ToString();
            if (string.IsNullOrEmpty(roomIdStr))
            {
                throw new HubException("roomId is null or empty");
            }

            var userId = Context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("UserId is null or empty");
            }

            var currentUser = await _userManager.FindByIdAsync(userId);
            if (currentUser == null)
            {
                throw new HubException($"User not found for UserId: {userId}");
            }

            var connectionId = Context?.ConnectionId;
            if (string.IsNullOrEmpty(connectionId))
            {
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
                    existingParticipants.Add(new { id = user.Id.ToString(), firstName = user.FirstName });
                }
            }

            await Clients.Caller.SendAsync("existingParticipants", existingParticipants);

            await Groups.AddToGroupAsync(connectionId, roomIdStr);
            _connectionManager.AddConnection(connectionId, userId, roomId);

            await Clients.OthersInGroup(roomIdStr).SendAsync("userJoined", new
            {
                id = currentUser.Id.ToString(),
                firstName = currentUser.FirstName
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
                await Clients.OthersInGroup(roomIdStr).SendAsync("userLeft", userId);
                Console.WriteLine($"User {userId} left room {roomIdStr}");
            }
            await base.OnDisconnectedAsync(exception);
        }
        
        public async Task sendOffer(string targetUserId, string serializedOffer)
        {
            var callerId = Context.UserIdentifier!;
            var targetConnectionId = await _connectionManager.GetConnectionIdAsync(targetUserId);
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("receiveOffer", callerId, serializedOffer);
            }
        }

        public async Task sendAnswer(string targetUserId, string serializedAnswer)
        {
            var targetConnectionId = await _connectionManager.GetConnectionIdAsync(targetUserId);
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("receiveAnswer", serializedAnswer);
            }
        }

        public async Task sendIceCandidate(string targetUserId, string candidate)
        {
            var targetConnectionId = await _connectionManager.GetConnectionIdAsync(targetUserId);
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("receiveIceCandidate", candidate);
            }
        }

        public async Task sendMessage(string messageContent)
        {
            try
            {
                var senderIdString = Context.UserIdentifier;
                if (string.IsNullOrEmpty(senderIdString) || !Guid.TryParse(senderIdString, out var senderId))
                {
                    Console.WriteLine("SendMessage Error: SenderId is not a valid Guid.");
                    return;
                }

                var roomIdString = await _connectionManager.GetRoomForConnectionAsync(Context.ConnectionId);

                if (string.IsNullOrEmpty(roomIdString) || !Guid.TryParse(roomIdString, out var roomId))
                {
                    Console.WriteLine($"SendMessage Error: RoomId could not be found for connection {Context.ConnectionId}.");
                    return;
                }

                var message = new Message
                {
                    Id = Guid.NewGuid(),
                    RoomId = roomId,
                    SenderId = senderId,
                    Content = messageContent,
                    SentAt = DateTime.UtcNow
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                var sender = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == senderId);
                if (sender == null)
                {
                    Console.WriteLine($"SendMessage Warning: Sender with Id {senderId} not found in Users table.");
                    return;
                }

                var senderNameParts = new[] { sender.FirstName, sender.MiddleName, sender.LastName };
                string senderName = string.Join(" ", senderNameParts.Where(s => !string.IsNullOrEmpty(s)));

                var messageDto = new
                {
                    id = message.Id,
                    roomId = message.RoomId,
                    senderId = message.SenderId,
                    content = message.Content,
                    sentAt = message.SentAt,
                    senderName
                };

                await Clients.Group(roomIdString).SendAsync("receiveMessage", messageDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!!!!! CRITICAL ERROR IN sendMessage !!!!!!");
                Console.WriteLine($"Exception: {ex}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"--- Inner Exception: {ex.InnerException}");
                }
                throw new HubException("An error occurred while sending the message.", ex);
            }
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