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

        var userRolesClaim = user.FindAll(ClaimTypes.Role)?.ToArray();
        if (userRolesClaim == null || !userRolesClaim.Any())
        {
            context.Result = new ForbidResult();
            return;
        }
        
        var isNoneRole = Enum.TryParse<RoleType>(userRolesClaim.FirstOrDefault()?.Value, out var parsedRole) 
                         && parsedRole == RoleType.None;

        if (isNoneRole)
        {
            context.Result = new ForbidResult();
            return;
        }

        var userRoles = RoleTypeExtensions.FromStringArray(userRolesClaim.Select(claim => claim.Value).ToArray());

        if (!userRoles.HasAnyRole(_requiredRoles))
        {
            context.Result = new ForbidResult();
        }
    }
}