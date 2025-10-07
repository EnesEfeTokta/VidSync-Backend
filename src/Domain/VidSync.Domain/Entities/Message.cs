namespace VidSync.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public User Sender { get; set; } = null!;
    public Room Room { get; set; } = null!;
}
