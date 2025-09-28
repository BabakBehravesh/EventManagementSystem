using EventManagementSystem.Application.DTOs.Auth;

namespace EventManagementSystem.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
    }
}
