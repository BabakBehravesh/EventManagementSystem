using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Application.DTOs.Auth;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace EventManagementSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<ApplicationUser> userManager,
                     RoleManager<IdentityRole> roleManager,
                     IJwtTokenGenerator jwtTokenGenerator,
                     IEmailService emailService,
                     ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ServiceResult<UserInfo>> RegisterAsync(RegisterRequest request, ApplicationUser registerer)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                return ServiceResult<UserInfo>.FailureResult("User with this email already exists.");
            }
            
            var user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = registerer.Email!
            };

            var password = GenerateRandomPassword();

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return ServiceResult<UserInfo>.FailureResult("Failed to create user.", result.Errors.Select(err => err.Description.ToString()).ToList());
            }

            foreach (var role in request.UserRoles)
            {
                await _userManager.AddToRoleAsync(user, role.ToString());
            }

            await _emailService.SendAccountCreatedEmailAsync(user.Email, user.UserName, password);
            await _emailService.SendWelcomeEmailAsync(user.Email, user.UserName);


            _logger.LogInformation($"User {user.Email} registered with roles: {string.Join(", ", request.UserRoles)}");


            UserInfo userInfo = new()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = [.. request.UserRoles.Select(r => r.ToString())]
            };

            return ServiceResult<UserInfo>.SuccessResult(userInfo, "User registered successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error registering user {request.Email}");
            return ServiceResult<UserInfo>.FailureResult("An error occurred while registering the user.");
        }
    }

    public async Task<ServiceResult<UserInfo>> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult<UserInfo>.FailureResult("User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Password changed successfully for user: {user.Email}");

                await _emailService.SendPasswordChangeConfirmationEmailAsync(user.Email, user.UserName);

                UserInfo userInfo = new()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                };

                return ServiceResult<UserInfo>.SuccessResult(userInfo, "Password changed successfully." );
            }

            return ServiceResult<UserInfo>.FailureResult("Failed to change password.", result.Errors.Select(err => err.Description).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error changing password for user {userId}");
            return ServiceResult<UserInfo>.FailureResult("An error occurred while changing password.");
        }
    }

    public async Task<ServiceResult<UserInfo>> ChangeUserProfileAsync(string userId, UserProfileRequest model)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult<UserInfo>.FailureResult("User not found.");
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;

        var result = await _userManager.UpdateAsync(user);
        
        UserInfo userInfo = new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };

        if (result.Succeeded)
        {
            return ServiceResult<UserInfo>.SuccessResult(userInfo, "User profile updated successfully.");
        }

        return ServiceResult<UserInfo>.FailureResult("Failed to update user profile.", result.Errors.Select(err => err.Description).ToList());
    }

    public async Task<ServiceResult<UserInfo>> LoadUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult<UserInfo>.FailureResult("User not found.");
        }

        user.FirstName = user.FirstName;
        user.LastName = user.LastName;

        UserInfo userInfo = new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };

        return ServiceResult<UserInfo>.SuccessResult(userInfo, "User profile updated successfully.");
    }

    public async Task<ServiceResult<UserInfo>> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult<UserInfo>.FailureResult("User not found.");
        }

        var result = await _userManager.DeleteAsync(user);
        
        UserInfo userInfo = new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };

        if (result.Succeeded)
        { 
            return ServiceResult<UserInfo>.SuccessResult(userInfo, $"User {user.Email} deleted successfully.");
        }

        return ServiceResult<UserInfo>.FailureResult($"Failed to delete user: {user.Email}", result.Errors.Select(err => err.Description).ToList());
    }

    public async Task<ServiceResult<UserInfo>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return ServiceResult<UserInfo>.FailureResult("Invalid login attempt.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        UserInfo userInfo = new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = roles.ToList(),
        }; 

        return ServiceResult<UserInfo>.AuthSuccessResult(userInfo, token, "Login successful!");
    }

    private static string GenerateRandomPassword()
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
        var random = new Random();
        var chars = new char[16];

        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = validChars[random.Next(validChars.Length)];
        }

        return new string(chars);
    }
}
