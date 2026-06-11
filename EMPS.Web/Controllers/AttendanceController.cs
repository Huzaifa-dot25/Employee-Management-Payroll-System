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
        private readonly IEmployeeService   _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AttendanceController(
            IAttendanceService attendanceService,
            IEmployeeService   employeeService,
            IDepartmentService departmentService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _attendanceService = attendanceService;
            _employeeService   = employeeService;
            _departmentService = departmentService;
            _userManager       = userManager;
            _mapper            = mapper;
        }

        // ── Attendance Log ────────────────────────────────────────────────────

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

        // ── Daily Attendance ──────────────────────────────────────────────────

        [Authorize(Roles = "Admin,HR")]
        [HttpGet]
        public async Task<IActionResult> Daily(DateTime? date)
        {
            var target = date?.Date ?? DateTime.Today;
            var records = await _attendanceService.GetByDateAsync(target);

            var model = new DailyAttendanceViewModel
            {
                Date    = target,
                Records = _mapper.Map<List<AttendanceViewModel>>(records)
            };
            return View(model);
        }

        // ── Monthly Attendance Report ─────────────────────────────────────────

        [Authorize(Roles = "Admin,HR")]
        [HttpGet]
        public async Task<IActionResult> MonthlyReport(int? month, int? year, int? departmentId)
        {
            var m = month ?? DateTime.Today.Month;
            var y = year  ?? DateTime.Today.Year;

            var records = await _attendanceService.GetMonthlyReportAsync(m, y, departmentId);

            var rows = records
                .GroupBy(a => a.EmployeeId)
                .Select(g =>
                {
                    var emp = g.First().Employee;
                    return new EmployeeMonthlyRow
                    {
                        EmployeeId    = g.Key,
                        EmployeeName  = emp?.FullName          ?? "—",
                        EmployeeCode  = emp?.EmployeeCode      ?? "—",
                        Department    = emp?.Department?.Name  ?? "—",
                        PresentDays   = g.Count(a => a.Status == "Present"),
                        AbsentDays    = g.Count(a => a.Status == "Absent"),
                        LeaveDays     = g.Count(a => a.Status == "Leave"),
                        LateDays      = g.Count(a => a.Status == "Late"),
                        HalfDays      = g.Count(a => a.Status == "HalfDay"),
                        TotalRecorded = g.Count()
                    };
                })
                .OrderBy(r => r.EmployeeName)
                .ToList();

            string deptName = "All Departments";
            if (departmentId.HasValue)
            {
                var dept = await _departmentService.GetDepartmentByIdAsync(departmentId.Value);
                deptName = dept?.Name ?? "All Departments";
            }

            var model = new MonthlyReportViewModel
            {
                Month          = m,
                Year           = y,
                DepartmentId   = departmentId,
                DepartmentName = deptName,
                Rows           = rows
            };

            await PopulateDepartmentsDropdownAsync(departmentId);
            return View(model);
        }

        // ── Employee Attendance History ───────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> EmployeeHistory(int? employeeId, DateTime? from, DateTime? to)
        {
            // Employees can only see their own history
            if (User.IsInRole("Employee"))
            {
                var currentEmpId = await GetCurrentEmployeeIdAsync();
                if (!currentEmpId.HasValue) return Forbid();
                employeeId = currentEmpId.Value;
            }

            if (!employeeId.HasValue)
            {
                // Admin/HR landed without selecting an employee — show picker
                await PopulateEmployeesDropdownAsync();
                return View(new EmployeeHistoryViewModel());
            }

            var records = await _attendanceService.GetEmployeeHistoryAsync(employeeId.Value, from, to);
            var emp     = (await _employeeService.GetAllEmployeesAsync())
                            .FirstOrDefault(e => e.Id == employeeId.Value);

            var model = new EmployeeHistoryViewModel
            {
                EmployeeId   = employeeId.Value,
                EmployeeName = emp?.FullName          ?? "—",
                EmployeeCode = emp?.EmployeeCode      ?? "—",
                Department   = emp?.Department?.Name  ?? "—",
                Designation  = emp?.Designation?.Name ?? "—",
                From         = from,
                To           = to,
                Records      = _mapper.Map<List<AttendanceViewModel>>(records)
            };

            await PopulateEmployeesDropdownAsync(employeeId);
            return View(model);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

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

        private async Task PopulateDepartmentsDropdownAsync(int? selectedDeptId = null)
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", selectedDeptId);
        }
    }
}
