using EventManagementSystem.Application.DTOs.Auth;
using EventManagementSystem.Domain.Models;

namespace EventManagementSystem.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request, ApplicationUser registerer);
        Task<AuthResult> LoginAsync(LoginRequest request);
    }
}
