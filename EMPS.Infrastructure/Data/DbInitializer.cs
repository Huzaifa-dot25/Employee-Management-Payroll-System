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

            // Seed some default Designations for IT Department if none exist
            var itDept = context.Departments.FirstOrDefault(d => d.Name == "IT");
            if (itDept != null && !context.Designations.Any(d => d.DepartmentId == itDept.Id))
            {
                context.Designations.AddRange(
                    new Designation { Name = "Software Engineer", SalaryGrade = "A", BasicSalary = 60000, DepartmentId = itDept.Id },
                    new Designation { Name = "Senior Software Engineer", SalaryGrade = "B", BasicSalary = 85000, DepartmentId = itDept.Id },
                    new Designation { Name = "System Administrator", SalaryGrade = "A", BasicSalary = 55000, DepartmentId = itDept.Id },
                    new Designation { Name = "IT Manager", SalaryGrade = "C", BasicSalary = 105000, DepartmentId = itDept.Id }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
