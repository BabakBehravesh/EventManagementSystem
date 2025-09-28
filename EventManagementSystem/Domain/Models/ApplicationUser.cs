using Microsoft.AspNetCore.Identity;

namespace EventManagementSystem.Domain.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();
}