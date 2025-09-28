using EventManagementSystem.Domain.Models;
using System.Security.Claims;

namespace EventManagementSystem.Domain.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user, IList<string> roles);

    ClaimsPrincipal ValidateToken(string token);
}
