using System.ComponentModel.DataAnnotations;

namespace VidSync.API.DTOs;

public class CreateRoomDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [DataType(DataType.DateTime)]
    public DateTime? ExpiresAt { get; set; }
}
