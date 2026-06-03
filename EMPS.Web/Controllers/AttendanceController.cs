using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IMapper _mapper;

        public AttendanceController(IAttendanceService attendanceService, IEmployeeService employeeService, IMapper mapper)
        {
            _attendanceService = attendanceService;
            _employeeService = employeeService;
            _mapper = mapper;
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
                // For normal employees, get their own attendance
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // Need to find EmployeeId for this user. We can load all employees and find it, or we assume there's a better way.
                // Assuming employee is linked via some claim or we get it via email.
                var employees = await _employeeService.GetAllEmployeesAsync();
                var email = User.Identity?.Name;
                var currentEmployee = employees.FirstOrDefault(e => e.Email == email);
                
                if (currentEmployee != null)
                {
                    attendances = await _attendanceService.GetAttendancesByEmployeeIdAsync(currentEmployee.Id);
                }
                else
                {
                    attendances = new List<Attendance>();
                }
            }
            
            var model = _mapper.Map<IEnumerable<AttendanceViewModel>>(attendances);
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateEmployeesDropdownAsync();
            return View(new AttendanceViewModel { Date = DateTime.Today });
        }
        
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
        
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
            if (attendance == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<AttendanceViewModel>(attendance);
            await PopulateEmployeesDropdownAsync(model.EmployeeId);
            return View(model);
        }
        
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

        private async Task PopulateEmployeesDropdownAsync(int? selectedEmployeeId = null)
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "FullName", selectedEmployeeId);
        }
    }
}
