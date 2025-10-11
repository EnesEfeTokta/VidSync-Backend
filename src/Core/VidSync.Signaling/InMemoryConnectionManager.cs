using System.Collections.Concurrent;

namespace VidSync.Signaling
{
    public class InMemoryConnectionManager : IConnectionManager
    {
        private readonly ConcurrentDictionary<string, string> _connectionToRoomMap = new();
        private readonly ConcurrentDictionary<string, string> _connectionToUserMap = new();
        private readonly ConcurrentDictionary<string, string> _userToConnectionMap = new();

        public void AddConnection(string connectionId, string userId, Guid roomId)
        {
            var roomIdStr = roomId.ToString();
            _connectionToUserMap[connectionId] = userId;
            _connectionToRoomMap[connectionId] = roomIdStr;
            _userToConnectionMap[userId] = connectionId;
        }

        public (string? UserId, string? RoomId) RemoveConnection(string connectionId)
        {
            _connectionToRoomMap.TryRemove(connectionId, out var roomId);
            if (_connectionToUserMap.TryRemove(connectionId, out var userId))
            {
                if (_userToConnectionMap.ContainsKey(userId) && _userToConnectionMap[userId] == connectionId)
                {
                    _userToConnectionMap.TryRemove(userId, out _);
                }
            }
            return (userId, roomId);
        }

        public Task<string?> GetConnectionIdAsync(string userId)
        {
            _userToConnectionMap.TryGetValue(userId, out var connectionId);
            return Task.FromResult(connectionId);
        }

        public Task<IEnumerable<string>> GetUsersInRoomAsync(Guid roomId, string? excludeConnectionId = null)
        {
            var roomIdStr = roomId.ToString();
            var users = _connectionToRoomMap
                .Where(kvp => kvp.Value == roomIdStr && kvp.Key != excludeConnectionId)
                .Select(kvp => _connectionToUserMap.GetValueOrDefault(kvp.Key))
                .Where(userId => userId != null)
                .Select(userId => userId!)
                .Distinct()
                .ToList();

            return Task.FromResult<IEnumerable<string>>(users);
        }

        public Task<string?> GetRoomForConnectionAsync(string connectionId)
        {
            _connectionToRoomMap.TryGetValue(connectionId, out var roomId);
            return Task.FromResult(roomId);
        }
    }
}