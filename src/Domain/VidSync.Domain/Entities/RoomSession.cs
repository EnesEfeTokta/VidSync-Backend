namespace VidSync.Domain.Entities;

public class RoomSession
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
