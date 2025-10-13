using Microsoft.AspNetCore.Identity;
using Vortex_API.Model.Domain;

namespace Vortex_API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "User" };

            foreach (var role in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
        {
            string adminEmail = "admin@vortex.com";
            string password = "Admin@123";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin"
                };
                await userManager.CreateAsync(admin, password);
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
