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
using EMPS.Web.Services;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Linq;

namespace EMPS.Web.Controllers
{
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly IPayrollService _payrollService;
        private readonly IEmployeeService _employeeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public PayrollController(
            IPayrollService payrollService,
            IEmployeeService employeeService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _payrollService  = payrollService;
            _employeeService = employeeService;
            _userManager     = userManager;
            _mapper          = mapper;
        }

        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Index()
        {
            var payrolls = await _payrollService.GetAllPayrollsAsync();
            var model = _mapper.Map<IEnumerable<PayrollViewModel>>(payrolls);
            return View(model);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateEmployeesDropdownAsync();
            return View(new PayrollViewModel { Month = DateTime.Today.Month, Year = DateTime.Today.Year });
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PayrollViewModel model)
        {
            if (ModelState.IsValid)
            {
                var payroll = _mapper.Map<Payroll>(model);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                await _payrollService.CreatePayrollAsync(payroll, userId);
                TempData["SuccessMessage"] = "Payroll record created successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateEmployeesDropdownAsync(model.EmployeeId);
            return View(model);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var payroll = await _payrollService.GetPayrollByIdAsync(id);
            if (payroll == null) return NotFound();

            var model = _mapper.Map<PayrollViewModel>(payroll);
            await PopulateEmployeesDropdownAsync(model.EmployeeId);
            return View(model);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PayrollViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var payroll = await _payrollService.GetPayrollByIdAsync(id);
                if (payroll == null) return NotFound();

                _mapper.Map(model, payroll);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                await _payrollService.UpdatePayrollAsync(payroll, userId);
                
                // If paid, generate payslip
                if (model.Status == "Paid" && payroll.Payslip == null)
                {
                    await _payrollService.GeneratePayslipAsync(payroll.Id, userId);
                }
                
                TempData["SuccessMessage"] = "Payroll updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateEmployeesDropdownAsync(model.EmployeeId);
            return View(model);
        }

        public async Task<IActionResult> MyPayslips()
        {
            var employeeId = await GetCurrentEmployeeIdAsync();

            if (employeeId.HasValue)
            {
                var payrolls = await _payrollService.GetPayrollsByEmployeeIdAsync(employeeId.Value);
                // Only show Approved or Paid payslips to employees
                payrolls = payrolls.Where(p => p.Status != "Draft").ToList();
                var model = _mapper.Map<IEnumerable<PayrollViewModel>>(payrolls);
                return View(model);
            }

            return View(new List<PayrollViewModel>());
        }

        // ── Download PDF ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> DownloadPayslip(int payrollId)
        {
            var payroll = await _payrollService.GetPayrollByIdAsync(payrollId);
            if (payroll == null) return NotFound();

            // Employees can only download their own payslip
            if (User.IsInRole("Employee"))
            {
                var empId = await GetCurrentEmployeeIdAsync();
                if (!empId.HasValue || empId.Value != payroll.EmployeeId)
                    return Forbid();
            }

            if (payroll.Status != "Paid" || payroll.Payslip == null)
                return BadRequest("Payslip is only available for Paid payrolls.");

            // Set QuestPDF community licence (free, no watermark for OSS/community use)
            QuestPDF.Settings.License = LicenseType.Community;

            var document  = new PayslipDocument(payroll);
            var pdfBytes  = document.GeneratePdf();
            var fileName  = $"Payslip-{payroll.Payslip.PayslipCode}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
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
