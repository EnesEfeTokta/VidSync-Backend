using System.ComponentModel.DataAnnotations;

namespace VidSync.API.DTOs;

public class UserRegisterDto
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [StringLength(50)]
    public string? MiddleName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }

    [Required]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = null!;
}
