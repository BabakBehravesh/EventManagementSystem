using EventManagementSystem.Application.Types;

namespace EventManagementSystem.Application.DTOs.Auth;

public class UserInfo
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public RoleType Roles { get; set; } = RoleType.None;

    public bool EmailConfirmed { get; set; }

    public string CreateBy { get; set; } = string.Empty;
}
