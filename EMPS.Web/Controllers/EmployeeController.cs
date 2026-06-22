using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EMPS.Core.Entities;
using EMPS.Core.Interfaces.Services;
using EMPS.Web.Models;

namespace EMPS.Web.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IDesignationService _designationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _hostEnvironment;

        public EmployeeController(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IDesignationService designationService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IWebHostEnvironment hostEnvironment)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _designationService = designationService;
            _userManager = userManager;
            _mapper = mapper;
            _hostEnvironment = hostEnvironment;
        }

        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            var model = _mapper.Map<IEnumerable<EmployeeViewModel>>(employees);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (User.IsInRole("Employee"))
            {
                var employeeId = await GetCurrentEmployeeIdAsync();
                if (employeeId != id)
                {
                    return Forbid();
                }
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<EmployeeViewModel>(employee);
            return View(model);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            var model = new EmployeeViewModel
            {
                EmployeeCode = await _employeeService.GenerateEmployeeCodeAsync()
            };
            return View(model);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Handle Profile Photo Upload
                string? photoPath = null;
                if (model.ProfileImage != null)
                {
                    photoPath = await UploadProfileImageAsync(model.ProfileImage);
                }

                var employee = _mapper.Map<Employee>(model);
                employee.ProfilePhotoPath = photoPath;

                // Create ApplicationUser automatically for Employee login
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = $"{model.FirstName} {model.LastName}",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createResult = await _userManager.CreateAsync(user, "Emp@123");
                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Employee");
                    employee.UserId = user.Id;
                }
                else
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, $"User creation failed: {error.Description}");
                    }
                    await PopulateDropdownsAsync(model.DepartmentId, model.DesignationId);
                    return View(model);
                }

                var creatorId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                await _employeeService.CreateEmployeeAsync(employee, creatorId);

                // Link User back to Employee record
                user.EmployeeId = employee.Id;
                await _userManager.UpdateAsync(user);

                TempData["SuccessMessage"] = "Employee and login account created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(model.DepartmentId, model.DesignationId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (User.IsInRole("Employee"))
            {
                var employeeId = await GetCurrentEmployeeIdAsync();
                if (employeeId != id)
                {
                    return Forbid();
                }
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<EmployeeViewModel>(employee);
            await PopulateDropdownsAsync(model.DepartmentId, model.DesignationId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (User.IsInRole("Employee"))
            {
                var employeeId = await GetCurrentEmployeeIdAsync();
                if (employeeId != id)
                {
                    return Forbid();
                }
            }

            if (ModelState.IsValid)
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return NotFound();
                }

                // Handle Photo Update
                string? photoPath = employee.ProfilePhotoPath;
                if (model.ProfileImage != null)
                {
                    // Delete old photo if exists
                    if (!string.IsNullOrEmpty(employee.ProfilePhotoPath))
                    {
                        DeleteOldImage(employee.ProfilePhotoPath);
                    }
                    photoPath = await UploadProfileImageAsync(model.ProfileImage);
                }

                // Update Employee details with role-based restrictions
                if (!User.IsInRole("Admin") && !User.IsInRole("HR"))
                {
                    // Restore non-editable details to prevent tampering
                    var originalCode = employee.EmployeeCode;
                    var originalDeptId = employee.DepartmentId;
                    var originalDesigId = employee.DesignationId;
                    var originalJoining = employee.JoiningDate;
                    var originalSalary = employee.BasicSalary;
                    var originalStatus = employee.EmploymentStatus;

                    _mapper.Map(model, employee);

                    employee.EmployeeCode = originalCode;
                    employee.DepartmentId = originalDeptId;
                    employee.DesignationId = originalDesigId;
                    employee.JoiningDate = originalJoining;
                    employee.BasicSalary = originalSalary;
                    employee.EmploymentStatus = originalStatus;
                }
                else
                {
                    _mapper.Map(model, employee);
                }

                employee.ProfilePhotoPath = photoPath;

                var updaterId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                await _employeeService.UpdateEmployeeAsync(employee, updaterId);

                // Update linked User FullName and Email if updated
                if (!string.IsNullOrEmpty(employee.UserId))
                {
                    var user = await _userManager.FindByIdAsync(employee.UserId);
                    if (user != null)
                    {
                        user.FullName = employee.FullName;
                        user.Email = employee.Email;
                        user.UserName = employee.Email;
                        await _userManager.UpdateAsync(user);
                    }
                }

                TempData["SuccessMessage"] = "Employee details updated successfully!";
                
                if (User.IsInRole("Employee"))
                {
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(model.DepartmentId, model.DesignationId);
            return View(model);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Deactivate linked User
            if (!string.IsNullOrEmpty(employee.UserId))
            {
                var user = await _userManager.FindByIdAsync(employee.UserId);
                if (user != null)
                {
                    user.IsActive = false;
                    await _userManager.UpdateAsync(user);
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            await _employeeService.DeleteEmployeeAsync(id, userId);

            TempData["SuccessMessage"] = "Employee deactivated successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync(int? departmentId = null, int? designationId = null)
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", departmentId);

            if (departmentId.HasValue)
            {
                var designations = await _designationService.GetDesignationsByDepartmentAsync(departmentId.Value);
                ViewBag.Designations = new SelectList(designations, "Id", "Name", designationId);
            }
            else
            {
                ViewBag.Designations = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

        private async Task<string> UploadProfileImageAsync(Microsoft.AspNetCore.Http.IFormFile imageFile)
        {
            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "profile_photos");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/uploads/profile_photos/" + uniqueFileName;
        }

        private void DeleteOldImage(string imagePath)
        {
            string fullPath = Path.Combine(_hostEnvironment.WebRootPath, imagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        private async Task<int?> GetCurrentEmployeeIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;
            var appUser = await _userManager.FindByIdAsync(userId);
            return appUser?.EmployeeId;
        }
    }
}
