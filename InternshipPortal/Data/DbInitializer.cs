using Microsoft.AspNetCore.Identity;

namespace InternshipPortal.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider
                    .GetRequiredService<
                        RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider
                    .GetRequiredService<
                        UserManager<IdentityUser>>();

            string[] roles =
            {
                "Admin",
                "Student",
                "Company",
                "UniversitySupervisor"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }

            await CreateUserWithRoleAsync(
                userManager,
                "admin@internship.com",
                "Admin123",
                "Admin");

            await CreateUserWithRoleAsync(
                userManager,
                "supervisor@internship.com",
                "Supervisor123",
                "UniversitySupervisor");
        }

        private static async Task CreateUserWithRoleAsync(
            UserManager<IdentityUser> userManager,
            string email,
            string password,
            string role)
        {
            var user =
                await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult =
                    await userManager.CreateAsync(
                        user,
                        password);

                if (!createResult.Succeeded)
                {
                    return;
                }
            }

            if (!await userManager.IsInRoleAsync(
                user,
                role))
            {
                await userManager.AddToRoleAsync(
                    user,
                    role);
            }
        }
    }
}