using EventManagementSystem.Application.Types;

namespace EventManagementSystem.Application.DTOs.Auth;

public class UpdateRolesRequest
{
    public string Email { get; set; } = string.Empty;
    public RoleType Roles { get; set; } = RoleType.None;
}
