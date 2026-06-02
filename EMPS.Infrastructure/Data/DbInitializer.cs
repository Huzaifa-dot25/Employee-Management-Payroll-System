using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using EMPS.Core.Entities;

namespace EMPS.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created
            context.Database.EnsureCreated();

            // Seed Roles
            string[] roleNames = { "Admin", "HR", "Employee" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Admin User
            var adminUser = await userManager.FindByEmailAsync("admin@emps.com");
            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "admin@emps.com",
                    Email = "admin@emps.com",
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createPowerUser = await userManager.CreateAsync(user, "Admin@123");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
