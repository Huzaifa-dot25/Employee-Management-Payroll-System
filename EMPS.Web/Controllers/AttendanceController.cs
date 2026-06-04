using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EMPS.Core.Entities;
using EMPS.Core.Interfaces.Services;
using EMPS.Web.Models;
using System.Linq;

namespace EMPS.Web.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IEmployeeService _employeeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AttendanceController(
            IAttendanceService attendanceService,
            IEmployeeService employeeService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _attendanceService = attendanceService;
            _employeeService   = employeeService;
            _userManager       = userManager;
            _mapper            = mapper;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Attendance> attendances;

            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                attendances = await _attendanceService.GetAllAttendancesAsync();
            }
            else
            {
                var employeeId = await GetCurrentEmployeeIdAsync();
                attendances = employeeId.HasValue
                    ? await _attendanceService.GetAttendancesByEmployeeIdAsync(employeeId.Value)
                    : new List<Attendance>();
            }

            var model = _mapper.Map<IEnumerable<AttendanceViewModel>>(attendances);
            return View(model);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateEmployeesDropdownAsync();
            return View(new AttendanceViewModel { Date = DateTime.Today });
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AttendanceViewModel model)
        {
            if (ModelState.IsValid)
            {
                var attendance = _mapper.Map<Attendance>(model);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                await _attendanceService.CreateAttendanceAsync(attendance, userId);
                TempData["SuccessMessage"] = "Attendance recorded successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateEmployeesDropdownAsync(model.EmployeeId);
            return View(model);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
            if (attendance == null) return NotFound();

            var model = _mapper.Map<AttendanceViewModel>(attendance);
            await PopulateEmployeesDropdownAsync(model.EmployeeId);
            return View(model);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AttendanceViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
                if (attendance == null) return NotFound();

                _mapper.Map(model, attendance);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                await _attendanceService.UpdateAttendanceAsync(attendance, userId);
                TempData["SuccessMessage"] = "Attendance updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateEmployeesDropdownAsync(model.EmployeeId);
            return View(model);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>Returns the EmployeeId linked to the currently logged-in user.</summary>
        private async Task<int?> GetCurrentEmployeeIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;

            var appUser = await _userManager.FindByIdAsync(userId);
            return appUser?.EmployeeId;
        }

        private async Task PopulateEmployeesDropdownAsync(int? selectedEmployeeId = null)
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "FullName", selectedEmployeeId);
        }
    }
}
