using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventManagementSystem.Domain.Models; // Make sure this namespace contains ApplicationUser

namespace EventManagementSystem.Infrastructure.Storage
{
    public class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var logger = scopedServices.GetRequiredService<ILogger<DbInitializer>>();

                logger.LogInformation("Starting database initialization...");

                // Use ApplicationUser instead of IdentityUser
                var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                var configuration = scopedServices.GetRequiredService<IConfiguration>();

                logger.LogInformation("Successfully got all services");

                // Create roles
                string[] roleNames = { "EventCreator", "EventParticipant" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Get admin credentials from configuration (Secret Manager)
                var adminUserName = configuration["AdminCredentials:UserName"];
                var adminEmail = configuration["AdminCredentials:Email"];
                var adminPassword = configuration["AdminCredentials:Password"];

                // Validate that credentials exist
                if (string.IsNullOrEmpty(adminUserName) ||
                    string.IsNullOrEmpty(adminEmail) ||
                    string.IsNullOrEmpty(adminPassword))
                {
                    throw new InvalidOperationException("Admin credentials are not configured. Please set them in Secret Manager.");
                }

                // Check if admin user already exists
                var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
                if (existingAdmin != null)
                {
                    // User already exists, ensure they have the correct role
                    if (!await userManager.IsInRoleAsync(existingAdmin, "EventCreator"))
                    {
                        await userManager.AddToRoleAsync(existingAdmin, "EventCreator");
                    }
                    return;
                }

                // Create the admin user - use ApplicationUser instead of IdentityUser
                ApplicationUser adminUser = new()
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "EventCreator");
                    logger.LogInformation("Admin user created successfully");
                }
                else
                {
                    // Log errors if user creation fails
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create admin user: {errors}");
                }
            }
        }
    }
}