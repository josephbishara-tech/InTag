using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Identity;
using Microsoft.AspNetCore.Identity;

namespace InTagWeb.Configuration
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Seed roles
            var roles = new[] { "Admin", "Manager", "Operator", "Viewer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = role,
                        Description = role switch
                        {
                            "Admin" => "Full access to all modules and settings",
                            "Manager" => "Create, edit, approve within assigned modules",
                            "Operator" => "View, perform inspections, record transactions",
                            "Viewer" => "Read-only access to assigned modules",
                            _ => role
                        }
                    });
                }
            }

            // Seed default admin
            var adminEmail = "admin@intag.local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin",
                    JobTitle = "Administrator",
                    TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.AddClaimAsync(adminUser,
                        new System.Security.Claims.Claim("TenantId", adminUser.TenantId.ToString()));
                }
            }
        }
    }
}
