using EventManagementSystem.Domain.Models;

namespace EventManagementSystem.Domain.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
