using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Application.DTOs.Auth;
using EventManagementSystem.Application.Services;
using EventManagementSystem.Application.Types;
using EventManagementSystem.Domain.Models;

namespace EventManagementSystem.Domain.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<UserInfo>> RegisterAsync(RegisterRequest request, ApplicationUser registerer);
    Task<ServiceResult<UserInfo>> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<ServiceResult<UserInfo>> LoginAsync(LoginRequest request);
    Task<ServiceResult<UserInfo>> ChangeUserProfileAsync(string userId, UserProfileRequest model);
    Task<ServiceResult<UserInfo>> LoadUserProfileAsync(string userId);
    Task<ServiceResult<UserInfo>> DeleteUserAsync(string userId);
    Task<ServiceResult<UserInfo>> UpdateUserRolesAsync(string userId, RoleType newRoles);
}
