namespace VidSync.Signaling
{
    public interface IConnectionManager
    {
        void AddConnection(string connectionId, string userId, Guid roomId);
        (string? UserId, string? RoomId) RemoveConnection(string connectionId);
        Task<string?> GetConnectionIdAsync(string userId);
        Task<IEnumerable<string>> GetUsersInRoomAsync(Guid roomId, string? excludeConnectionId = null);
        Task<string?> GetRoomForConnectionAsync(string connectionId);
    }
}