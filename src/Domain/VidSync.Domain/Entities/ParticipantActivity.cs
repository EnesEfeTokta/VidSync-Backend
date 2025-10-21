namespace VidSync.Domain.Entities;

public class ParticipantActivity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoomSessionId { get; set; }
    public RoomSession RoomSession { get; set; } = null!;
    public string IpAddress { get; set; } = null!;
    public DateTime JoinTime { get; set; }
    public DateTime? LeaveTime { get; set; }
}
