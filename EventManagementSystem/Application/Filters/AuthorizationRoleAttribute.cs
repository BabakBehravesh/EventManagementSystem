using EventManagementSystem.Application.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace EventManagementSystem.Application.Filters;

public class AuthorizeRoleAttribute(RoleType requiredRoles) : Attribute, IAuthorizationFilter
{
    private readonly RoleType _requiredRoles = requiredRoles;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        var userRolesClaim = user.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(userRolesClaim))
        {
            context.Result = new ForbidResult();
            return;
        }

        var userRoles = userRolesClaim.Split(',')
            .Select(role => Enum.Parse<RoleType>(role))
            .Aggregate(RoleType.None, (current, role) => current | role);

        if (!userRoles.HasAnyRole(_requiredRoles))
        {
            context.Result = new ForbidResult();
        }
    }
}