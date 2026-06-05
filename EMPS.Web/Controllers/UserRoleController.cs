using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EMPS.Core.Entities;
using EMPS.Core.Interfaces.Services;
using EMPS.Web.Models;

namespace EMPS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserRoleController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmployeeService _employeeService;

        public UserRoleController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmployeeService employeeService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _employeeService = employeeService;
        }

        // GET: /UserRole
        public async Task<IActionResult> Index()
        {
            var allUsers  = _userManager.Users.ToList();
            var employees = (await _employeeService.GetAllEmployeesAsync()).ToList(); // already includes Dept/Desig
            var model     = new List<UserRoleViewModel>();

            foreach (var user in allUsers)
            {
                var roles    = await _userManager.GetRolesAsync(user);
                var empMatch = employees.FirstOrDefault(e => e.UserId == user.Id);

                model.Add(new UserRoleViewModel
                {
                    UserId        = user.Id,
                    FullName      = user.FullName,
                    Email         = user.Email ?? "",
                    CurrentRole   = roles.FirstOrDefault() ?? "—",
                    IsActive      = user.IsActive,
                    EmployeeId    = empMatch?.Id,
                    EmployeeCode  = empMatch?.EmployeeCode,
                    DepartmentName  = empMatch?.Department?.Name,
                    DesignationName = empMatch?.Designation?.Name
                });
            }

            // Sort: Admin first, then HR, then Employee, then unassigned
            model = model
                .OrderBy(m => m.CurrentRole == "Admin"    ? 0 :
                              m.CurrentRole == "HR"       ? 1 :
                              m.CurrentRole == "Employee" ? 2 : 3)
                .ThenBy(m => m.FullName)
                .ToList();

            return View(model);
        }

        // GET: /UserRole/Assign/userId
        [HttpGet]
        public async Task<IActionResult> Assign(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var allRoles     = _roleManager.Roles.Select(r => r.Name!).OrderBy(r => r).ToList();
            var employees    = await _employeeService.GetAllEmployeesAsync();
            var empMatch     = employees.FirstOrDefault(e => e.UserId == user.Id);

            var model = new AssignRoleViewModel
            {
                UserId          = user.Id,
                FullName        = user.FullName,
                Email           = user.Email ?? "",
                CurrentRole     = currentRoles.FirstOrDefault() ?? "—",
                SelectedRole    = currentRoles.FirstOrDefault() ?? "",
                AvailableRoles  = allRoles,
                EmployeeId      = empMatch?.Id,
                EmployeeCode    = empMatch?.EmployeeCode,
                DepartmentName  = empMatch?.Department?.Name,
                DesignationName = empMatch?.Designation?.Name
            };

            return View(model);
        }

        // POST: /UserRole/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = _roleManager.Roles.Select(r => r.Name!).OrderBy(r => r).ToList();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            // Prevent demoting the last Admin
            if (model.CurrentRole == "Admin" && model.SelectedRole != "Admin")
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["ErrorMessage"] = "Cannot remove the last Admin. Assign another Admin first.";
                    return RedirectToAction(nameof(Assign), new { id = model.UserId });
                }
            }

            // Remove all existing roles then assign the new one
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            await _userManager.AddToRoleAsync(user, model.SelectedRole);

            TempData["SuccessMessage"] = $"{user.FullName}'s role has been updated to <strong>{model.SelectedRole}</strong>.";
            return RedirectToAction(nameof(Index));
        }
    }
}
