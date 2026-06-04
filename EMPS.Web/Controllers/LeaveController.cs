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
    public class LeaveController : Controller
    {
        private readonly ILeaveService _leaveService;
        private readonly IEmployeeService _employeeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public LeaveController(
            ILeaveService leaveService,
            IEmployeeService employeeService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _leaveService    = leaveService;
            _employeeService = employeeService;
            _userManager     = userManager;
            _mapper          = mapper;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<LeaveRequest> leaves;

            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                leaves = await _leaveService.GetAllLeaveRequestsAsync();
            }
            else
            {
                var employeeId = await GetCurrentEmployeeIdAsync();
                leaves = employeeId.HasValue
                    ? await _leaveService.GetLeaveRequestsByEmployeeIdAsync(employeeId.Value)
                    : new List<LeaveRequest>();
            }

            var model = _mapper.Map<IEnumerable<LeaveRequestViewModel>>(leaves);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new LeaveRequestViewModel { StartDate = DateTime.Today, EndDate = DateTime.Today };

            if (User.IsInRole("Admin") || User.IsInRole("HR"))
            {
                await PopulateEmployeesDropdownAsync();
            }
            else
            {
                var employeeId = await GetCurrentEmployeeIdAsync();
                if (employeeId.HasValue)
                    model.EmployeeId = employeeId.Value;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var leaveRequest = _mapper.Map<LeaveRequest>(model);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                await _leaveService.CreateLeaveRequestAsync(leaveRequest, userId);
                TempData["SuccessMessage"] = "Leave request submitted successfully.";
                return RedirectToAction(nameof(Index));
            }
            if (User.IsInRole("Admin") || User.IsInRole("HR"))
                await PopulateEmployeesDropdownAsync(model.EmployeeId);
            return View(model);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var leaveRequest = await _leaveService.GetLeaveRequestByIdAsync(id);
            if (leaveRequest == null) return NotFound();

            var model = _mapper.Map<LeaveRequestViewModel>(leaveRequest);
            await PopulateEmployeesDropdownAsync(model.EmployeeId);
            return View(model);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LeaveRequestViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var leaveRequest = await _leaveService.GetLeaveRequestByIdAsync(id);
                if (leaveRequest == null) return NotFound();

                _mapper.Map(model, leaveRequest);

                if (model.Status == "Approved" || model.Status == "Rejected")
                {
                    leaveRequest.ApprovedBy = User.Identity?.Name;
                    leaveRequest.ApprovedAt = DateTime.UtcNow;
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                await _leaveService.UpdateLeaveRequestAsync(leaveRequest, userId);
                TempData["SuccessMessage"] = "Leave request updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateEmployeesDropdownAsync(model.EmployeeId);
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
    }
}
