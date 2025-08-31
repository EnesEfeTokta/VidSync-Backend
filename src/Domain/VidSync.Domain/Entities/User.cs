using Microsoft.AspNetCore.Identity;

namespace VidSync.Domain.Entities;

public class User : IdentityUser<Guid>
{
    // Basic user information is inherited from IdentityUser.
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ProfilePictureUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
