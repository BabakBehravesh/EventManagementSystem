using Microsoft.AspNetCore.Identity;
using EventManagementSystem.Domain.Models;
using EventManagementSystem.Application.Types;

namespace EventManagementSystem.Infrastructure.Storage;

public class DbInitializer(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ILogger<DbInitializer> logger
        )
{
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<DbInitializer> _logger = logger;

    public async Task Initialize(IServiceProvider serviceProvider)
    {
        _logger.LogInformation("Starting database initialization...");
        _logger.LogInformation("Creating roles database initialization...");

        string[] roleNames = Enum.GetNames(typeof(RoleType));
        foreach (var roleName in roleNames)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        _logger.LogInformation("Roles created if not created before.");

        _logger.LogInformation("Getting admin credentials from configuration (Secret Manager)...");
        var adminUserName = _configuration["DefaultAdminCredentials:UserName"];
        var adminEmail = _configuration["DefaultAdminCredentials:Email"];
        var adminPassword = _configuration["DefaultAdminCredentials:Password"];

        if (string.IsNullOrEmpty(adminUserName) ||
            string.IsNullOrEmpty(adminEmail) ||
            string.IsNullOrEmpty(adminPassword))
        {
            _logger.LogError("The admin credentials are not configured. Please set them in Secret Manager.");
            throw new InvalidOperationException("Admin credentials are not configured. Please set them in Secret Manager.");
        }
        
        _logger.LogInformation("Check if admin user already exists...");
        var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin != null)
        {
            _logger.LogWarning("User already exists, ensure they have the correct role");
            foreach(var role in roleNames)
            { 
                if (!await _userManager.IsInRoleAsync(existingAdmin, role))
                {
                    await _userManager.AddToRoleAsync(existingAdmin, role);
                    _logger.LogWarning($"Admin user is assigned to role: {role}");
                }
            }
            return;
        }

        _logger.LogInformation("Creating admin user...");
        ApplicationUser adminUser = new()
        {
            FirstName = "Admin",
            LastName = "Admin",
            UserName = adminUserName,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(adminUser, RoleType.EventCreator.ToString());
            _logger.LogInformation("Admin user created successfully");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create admin user");
            throw new InvalidOperationException($"Failed to create admin user: {errors}");
        }            
    }
}