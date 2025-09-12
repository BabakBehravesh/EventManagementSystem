using Microsoft.AspNetCore.Identity;

namespace EventManagementSystem.Infrastructure.Storage
{
    public class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Create roles
            string[] roleNames = { "EventCreator", "EventParticipant" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create the default admin/event creator user
            IdentityUser adminUser = new()
            {
                UserName = "admin@docuware.com",
                Email = "admin@docuware.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "YourStrongPassword1!");
            if (result.Succeeded)
            {
                // Assign the user to the "EventCreator" role
                await userManager.AddToRoleAsync(adminUser, "EventCreator");
            }

        }
    }
}

