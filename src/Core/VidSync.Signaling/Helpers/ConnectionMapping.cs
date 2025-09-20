namespace VidSync.Signaling.Helpers;

public class ConnectionMapping<T> where T : notnull
{
    private readonly Dictionary<string, T> _connectionToRoom = new Dictionary<string, T>();

    public void Add(string connectionId, T roomId)
    {
        lock (_connectionToRoom)
        {
            _connectionToRoom[connectionId] = roomId;
        }
    }
        
    public bool TryGetRoom(string connectionId, out T? roomId)
    {
        lock (_connectionToRoom)
        {
            return _connectionToRoom.TryGetValue(connectionId, out roomId);
        }
    }

    public void Remove(string connectionId)
    {
        lock (_connectionToRoom)
        {
            _connectionToRoom.Remove(connectionId);
        }
    }
}
