namespace EventManagementSystem.Application.Types;

public static class RoleTypeExtensions
{
    public static bool HasRole(this RoleType userRoles, RoleType role)
    {
        return (userRoles & role) == role;
    }

    public static bool HasAnyRole(this RoleType userRoles, RoleType roles)
    {
        return (userRoles & roles) != RoleType.None;
    }

    public static bool HasAllRoles(this RoleType userRoles, RoleType requiredRoles)
    {
        if (requiredRoles == RoleType.None)
        {
           return userRoles == RoleType.None;
        }

        return (userRoles & requiredRoles) == requiredRoles;
    }

    public static RoleType AddRole(this RoleType userRoles, RoleType roleToAdd)
    {
        if (roleToAdd == RoleType.None)
        {
            return userRoles;
        }

        return userRoles | roleToAdd;
    }
    
    public static RoleType RemoveRole(this RoleType userRoles, RoleType roleToRemove)
    {
        if (roleToRemove == RoleType.None)
        {
            return userRoles;
        }
        return userRoles & ~roleToRemove;
    }

    public static string[] ToStringArray(this RoleType userRoles)
    {
        return Enum.GetValues<RoleType>()
            .Where(role => role != RoleType.None && userRoles.HasRole(role))
            .Select(role => role.ToString())
            .ToArray();
    }

    public static string ToRoleString(this RoleType userRoles)
    {
        if (userRoles == RoleType.None)
        {
            return "None";
        }

        return string.Join(",", userRoles.ToStringArray());
    }

    public static RoleType FromStringArray(string[] roles)
    {
        RoleType result = RoleType.None;
        foreach (var role in roles)
        {
            if (Enum.TryParse<RoleType>(role, out var parsedRole))
            {
                result = result.AddRole(parsedRole);
            }
        }
        return result;
    }

    public static bool HasExactlyRoles(this RoleType userRoles, RoleType requiredRoles)
    {
        return userRoles == requiredRoles;
    }
}
