using EventManagementSystem.Application.DTOs;

namespace EventManagementSystem.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task<AuthResult> ChangePasswordAsync(string userId, ChangePasswordRequest request);
        Task<AuthResult> UpdateProfileAsync(string userId, UpdateProfileRequest request);
        Task<AuthResult> GetUserProfileAsync(string userId);
        Task<AuthResult> AssignRolesAsync(string userId, List<string> roles, string currentUserId);
        Task<AuthResult> ForgotPasswordAsync(string email);
        Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request);
        Task<AuthResult> ValidateResetTokenAsync(string email, string token);
        Task<AuthResult> GeneratePasswordResetTokenAsync(string email);
    }
}
