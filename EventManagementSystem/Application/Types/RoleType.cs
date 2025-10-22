
namespace EventManagementSystem.Application.Types;

[Flags]
public enum RoleType
{
    None = 0,
    Admin = 1 << 0,
    EventCreator = 1 << 1,
    EventParticipant = 1 << 2,
}
