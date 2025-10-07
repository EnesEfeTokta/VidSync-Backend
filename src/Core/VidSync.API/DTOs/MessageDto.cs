namespace VidSync.API.DTOs;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public string SenderName { get; set; } = null!;
}
