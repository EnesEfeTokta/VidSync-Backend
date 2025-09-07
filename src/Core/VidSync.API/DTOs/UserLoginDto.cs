using System.ComponentModel.DataAnnotations;

namespace VidSync.API.DTOs;

public class UserLoginDto
{
    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}