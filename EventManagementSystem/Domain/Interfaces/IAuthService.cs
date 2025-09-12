using EventManagementSystem.Application.DTOs;

namespace EventManagementSystem.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task SeedAdminUserAsync();
    }
}
