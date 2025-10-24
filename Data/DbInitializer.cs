using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace CMCS.Data
{
    public static class DbInitializer
    {
        private static readonly string[] Roles = new[] { "Lecturer", "ProgrammeCoordinator", "AcademicManager" };

        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // seed sample users
            await CreateUserIfNotExists(userManager, "lecturer@test.com", "Lecturer1!", "Lecturer");
            await CreateUserIfNotExists(userManager, "coordinator@test.com", "Coordinator1!", "ProgrammeCoordinator");
            await CreateUserIfNotExists(userManager, "manager@test.com", "Manager1!", "AcademicManager");
        }

        private static async Task CreateUserIfNotExists(UserManager<IdentityUser> userManager, string email, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
