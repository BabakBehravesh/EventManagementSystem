using EventManagementSystem.Application.DTOs.Auth;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace EventManagementSystem.Application.Services
{
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

        public async Task<AuthResult> RegisterAsync(RegisterRequest request, ApplicationUser registerer)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);

                if (existingUser != null)
                {
                    return AuthResult.FailureResult("User with this email already exists.");
                }

                var password = GenerateRandomPassword();
                
                var user = new ApplicationUser
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    UserName = request.Email,
                    Email = request.Email,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = registerer.Email!
                };

                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    return AuthResult.FailureResult("Failed to create user.", result.Errors);
                }

                foreach (var role in request.UserRoles)
                {
                    await _userManager.AddToRoleAsync(user, role.ToString());
                }

                await _emailService.SendAccountCreatedEmailAsync(user.Email, user.UserName, password);
                await _emailService.SendWelcomeEmailAsync(user.Email, user.UserName);


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
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return AuthResult.FailureResult("Invalid login attempt.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            return AuthResult.SuccessResult("Login successful!", token);
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
}
