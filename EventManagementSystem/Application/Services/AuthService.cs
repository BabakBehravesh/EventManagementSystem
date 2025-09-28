using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace EventManagementSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailService emailService,
        ILogger<AuthService> logger,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return AuthResult.FailureResult("User with this email already exists.");
            }

            // Validate roles - USING Task.WhenAll FOR CONCURRENT CHECKS
            var roleCheckTasks = request.UserRoles.Select(async role => new
            {
                Role = role,
                Exists = await _roleManager.RoleExistsAsync(role.ToString())
            }).ToList();

            var roleCheckResults = await Task.WhenAll(roleCheckTasks);
            var invalidRoles = roleCheckResults
                .Where(result => !result.Exists)
                .Select(result => result.Role)
                .ToList();

            if (invalidRoles.Any())
            {
                return AuthResult.FailureResult($"Invalid roles: {string.Join(", ", invalidRoles)}");
            }

            var password = request.Password;
            var isTemporaryPassword = false;

            if (string.IsNullOrEmpty(password))
            {
                password = GenerateRandomPassword();
                isTemporaryPassword = true;
            }

            var user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return AuthResult.FailureResult("Failed to create user.", result.Errors);
            }

            // Add user to roles
            foreach (var role in request.UserRoles)
            {
                await _userManager.AddToRoleAsync(user, role.ToString());
            }

            // Send appropriate email based on password type
            if (isTemporaryPassword)
            {
                await _emailService.SendAccountCreatedEmailAsync(user.Email, user.UserName, password);
            }
            else
            {
                await _emailService.SendWelcomeEmailAsync(user.Email, user.UserName);
            }

            _logger.LogInformation($"User {user.Email} registered with roles: {string.Join(", ", request.UserRoles)}");

            return AuthResult.SuccessResult("User registered successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error registering user {request.Email}");
            return AuthResult.FailureResult("An error occurred while registering the user.");
        }
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogWarning($"Failed login attempt for email: {request.Email}");
                return AuthResult.FailureResult("Invalid login attempt.");
            }

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                return AuthResult.FailureResult("Please confirm your email address before logging in.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            _logger.LogInformation($"User {user.Email} logged in successfully. Roles: {string.Join(", ", roles)}");

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                EmailConfirmed = user.EmailConfirmed
            };

            return AuthResult.SuccessResult("Login successful!", token, userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during login for {request.Email}");
            return AuthResult.FailureResult("An error occurred during login.");
        }
    }

    public async Task<AuthResult> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return AuthResult.FailureResult("User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Password changed successfully for user: {user.Email}");

                // Send password change confirmation email
                await _emailService.SendPasswordChangeConfirmationEmailAsync(user.Email, user.UserName);

                return AuthResult.SuccessResult("Password changed successfully.");
            }

            return AuthResult.FailureResult("Failed to change password.", result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error changing password for user {userId}");
            return AuthResult.FailureResult("An error occurred while changing password.");
        }
    }

    public async Task<AuthResult> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return AuthResult.FailureResult("User not found.");
            }

            user.UserName = request.UserName;
            user.PhoneNumber = request.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Profile updated successfully for user: {user.Email}");
                return AuthResult.SuccessResult("Profile updated successfully.");
            }

            return AuthResult.FailureResult("Failed to update profile.", result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating profile for user {userId}");
            return AuthResult.FailureResult("An error occurred while updating profile.");
        }
    }

    public async Task<AuthResult> GetUserProfileAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return AuthResult.FailureResult("User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                EmailConfirmed = user.EmailConfirmed
            };

            return AuthResult.SuccessResult("Profile retrieved successfully.", userInfo: userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting profile for user {userId}");
            return AuthResult.FailureResult("An error occurred while retrieving profile.");
        }
    }

    public async Task<AuthResult> AssignRolesAsync(string userId, List<string> roles, string currentUserId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return AuthResult.FailureResult("User not found.");
            }

            // Validate roles - FIXED VERSION
            var invalidRoles = new List<string>();
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    invalidRoles.Add(role);
                }
            }

            if (invalidRoles.Any())
            {
                return AuthResult.FailureResult($"Invalid roles: {string.Join(", ", invalidRoles)}");
            }

            // Remove existing roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Add new roles
            var result = await _userManager.AddToRolesAsync(user, roles);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Roles assigned successfully for user: {user.Email}. Roles: {string.Join(", ", roles)}");

                // Send notification email about role changes
                await SendRoleAssignmentEmailAsync(user, roles);

                return AuthResult.SuccessResult("Roles assigned successfully.");
            }

            return AuthResult.FailureResult("Failed to assign roles.", result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error assigning roles for user {userId}");
            return AuthResult.FailureResult("An error occurred while assigning roles.");
        }
    }

    public async Task<AuthResult> ForgotPasswordAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                // Don't reveal that the user doesn't exist or is not confirmed
                _logger.LogInformation($"Password reset requested for non-existent or unconfirmed email: {email}");
                return AuthResult.SuccessResult("If your email is registered, you will receive a password reset link.");
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Encode token for URL safety
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Create reset URL
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "https://yourapp.com";
            var callbackUrl = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={encodedToken}";

            // Send password reset email
            await _emailService.SendPasswordResetEmailAsync(user.Email, user.UserName, callbackUrl);

            _logger.LogInformation($"Password reset email sent to {email}");

            return AuthResult.SuccessResult("If your email is registered, you will receive a password reset link.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing forgot password request for {email}");
            return AuthResult.FailureResult("An error occurred while processing your request.");
        }
    }

    public async Task<AuthResult> GeneratePasswordResetTokenAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return AuthResult.FailureResult("User not found.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return new AuthResult(true, token: encodedToken, message: "Password reset token generated successfully.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating password reset token for {email}");
            return AuthResult.FailureResult("An error occurred while generating password reset token.");
        }
    }

    public async Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return AuthResult.FailureResult("Invalid password reset attempt.");
            }

            // Decode the token
            byte[] tokenData;
            try
            {
                tokenData = WebEncoders.Base64UrlDecode(request.Token);
            }
            catch (FormatException)
            {
                return AuthResult.FailureResult("Invalid reset token.");
            }

            var decodedToken = Encoding.UTF8.GetString(tokenData);

            // Reset password
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Password reset successfully for user: {request.Email}");

                // Send password change confirmation email
                await _emailService.SendPasswordChangeConfirmationEmailAsync(user.Email, user.UserName);

                return AuthResult.SuccessResult("Password has been reset successfully.");
            }

            var errors = result.Errors.Select(e => e.Description);
            _logger.LogWarning($"Password reset failed for {request.Email}. Errors: {string.Join(", ", errors)}");

            return AuthResult.FailureResult("Failed to reset password.", result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resetting password for {request.Email}");
            return AuthResult.FailureResult("An error occurred while resetting your password.");
        }
    }

    public async Task<AuthResult> ValidateResetTokenAsync(string email, string token)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return AuthResult.FailureResult("Invalid token.");
            }

            // Decode the token
            byte[] tokenData;
            try
            {
                tokenData = WebEncoders.Base64UrlDecode(token);
            }
            catch (FormatException)
            {
                return AuthResult.FailureResult("Invalid token format.");
            }

            var decodedToken = Encoding.UTF8.GetString(tokenData);

            // Validate the token
            var isValid = await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword",
                decodedToken);

            return new AuthResult(isValid, isValid ? "Token is valid." : "Invalid or expired token.", token, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error validating reset token for {email}");
            return AuthResult.FailureResult("Error validating token.");
        }
    }

    private async Task SendRoleAssignmentEmailAsync(ApplicationUser user, List<string> newRoles)
    {
        try
        {
            var subject = "Your Account Roles Have Been Updated";
            var htmlContent = $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1 style='color: #3366cc;'>Account Roles Updated</h1>
        <p>Hello {user.UserName},</p>
        <p>Your account roles have been updated. You now have the following roles:</p>
        <ul>
            {string.Join("", newRoles.Select(role => $"<li><strong>{role}</strong></li>"))}
        </ul>
        <p>These roles determine what actions you can perform in the system.</p>
        <p>If you believe this is an error, please contact our support team.</p>
        <p>Best regards,<br><strong>The Event Management System Team</strong></p>
    </div>
</body>
</html>";

            await _emailService.SendCustomEmailAsync(user.Email, user.UserName, subject, htmlContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending role assignment email to {user.Email}");
            // Don't throw, just log the error
        }
    }

    private string GenerateRandomPassword()
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
        var random = new Random();
        var chars = new char[12];

        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = validChars[random.Next(validChars.Length)];
        }

        return new string(chars);
    }
}