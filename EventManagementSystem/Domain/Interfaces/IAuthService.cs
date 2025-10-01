using EventManagementSystem.Application.DTOs.Auth;
using EventManagementSystem.Domain.Models;

namespace EventManagementSystem.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request, ApplicationUser registerer);
        Task<AuthResult> ChangePasswordAsync(string userId, ChangePasswordRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task<AuthResult> ChangeUserProfileAsync(string userId, UserProfileRequest model);
        Task<AuthResult> LoadUserProfileAsync(string userId);
        Task<AuthResult> DeleteUserAsync(string userId);
    }
}
