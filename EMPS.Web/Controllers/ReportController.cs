using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EMPS.Core.Interfaces;
using EMPS.Core.Entities;
using EMPS.Web.Services;
using QuestPDF.Fluent;

namespace EMPS.Web.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class ReportController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportEmployeeReport(int? departmentId, string? status, string format)
        {
            var employees = await _unitOfWork.Employees.GetAllWithIncludesAsync(
                e => e.Department,
                e => e.Designation);

            if (departmentId.HasValue)
                employees = employees.Where(e => e.DepartmentId == departmentId.Value);

            if (!string.IsNullOrEmpty(status) && status != "All")
                employees = employees.Where(e => e.EmploymentStatus.Equals(status, StringComparison.OrdinalIgnoreCase));

            if (format.ToLower() == "csv")
            {
                var sb = new StringBuilder();
                sb.AppendLine("Code,Name,Email,Phone,Gender,Date of Birth,Joining Date,Status,Department,Designation,Basic Salary,Bank,Account Number,Tax ID");
                foreach (var emp in employees)
                {
                    sb.AppendLine($"{EscapeCsv(emp.EmployeeCode)}," +
                                  $"{EscapeCsv(emp.FullName)}," +
                                  $"{EscapeCsv(emp.Email)}," +
                                  $"{EscapeCsv(emp.PhoneNumber)}," +
                                  $"{EscapeCsv(emp.Gender)}," +
                                  $"{emp.DateOfBirth:yyyy-MM-dd}," +
                                  $"{emp.JoiningDate:yyyy-MM-dd}," +
                                  $"{EscapeCsv(emp.EmploymentStatus)}," +
                                  $"{EscapeCsv(emp.Department?.Name)}," +
                                  $"{EscapeCsv(emp.Designation?.Name)}," +
                                  $"{emp.BasicSalary}," +
                                  $"{EscapeCsv(emp.BankName)}," +
                                  $"{EscapeCsv(emp.BankAccountNumber)}," +
                                  $"{EscapeCsv(emp.TaxIdentificationNumber)}");
                }
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                return File(bytes, "text/csv", $"EmployeeReport_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            else
            {
                var doc = new EmployeeReportDocument(employees);
                var pdfBytes = doc.GeneratePdf();
                return File(pdfBytes, "application/pdf", $"EmployeeReport_{DateTime.UtcNow:yyyyMMdd}.pdf");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportAttendanceReport(DateTime fromDate, DateTime toDate, int? departmentId, string format)
        {
            var attendances = await _unitOfWork.Attendances.FindWithIncludesAsync(
                a => a.Date.Date >= fromDate.Date && a.Date.Date <= toDate.Date,
                a => a.Employee);

            if (departmentId.HasValue)
                attendances = attendances.Where(a => a.Employee.DepartmentId == departmentId.Value);

            // Re-fetch or load employee's department and designation details if necessary
            // Since they might not be fully loaded, let's ensure we can access the Department Name.
            // In Entity Framework Core, loaded references will populate. Since we loaded them in Employees,
            // let's fetch department reference in-memory if needed. Actually we can do:
            var allDepts = (await _unitOfWork.Departments.GetAllAsync()).ToDictionary(d => d.Id, d => d.Name);
            foreach (var a in attendances)
            {
                if (a.Employee != null && a.Employee.Department == null && allDepts.ContainsKey(a.Employee.DepartmentId))
                {
                    a.Employee.Department = new Department { Name = allDepts[a.Employee.DepartmentId] };
                }
            }

            attendances = attendances.OrderBy(a => a.Date).ThenBy(a => a.Employee?.FirstName);

            if (format.ToLower() == "csv")
            {
                var sb = new StringBuilder();
                sb.AppendLine("Date,Employee Code,Employee Name,Status,Check In,Check Out,Remarks");
                foreach (var att in attendances)
                {
                    sb.AppendLine($"{att.Date:yyyy-MM-dd}," +
                                  $"{EscapeCsv(att.Employee?.EmployeeCode)}," +
                                  $"{EscapeCsv(att.Employee?.FullName)}," +
                                  $"{EscapeCsv(att.Status)}," +
                                  $"{att.CheckInTime?.ToString(@"hh\:mm") ?? "—"}," +
                                  $"{att.CheckOutTime?.ToString(@"hh\:mm") ?? "—"}," +
                                  $"{EscapeCsv(att.Remarks)}");
                }
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                return File(bytes, "text/csv", $"AttendanceReport_{fromDate:yyyyMMdd}_to_{toDate:yyyyMMdd}.csv");
            }
            else
            {
                var doc = new AttendanceReportDocument(attendances, fromDate, toDate);
                var pdfBytes = doc.GeneratePdf();
                return File(pdfBytes, "application/pdf", $"AttendanceReport_{fromDate:yyyyMMdd}_to_{toDate:yyyyMMdd}.pdf");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportPayrollReport(int month, int year, int? departmentId, string format)
        {
            var payrolls = await _unitOfWork.Payrolls.FindWithIncludesAsync(
                p => p.Month == month && p.Year == year,
                p => p.Employee);

            var allDepts = (await _unitOfWork.Departments.GetAllAsync()).ToDictionary(d => d.Id, d => d.Name);
            foreach (var p in payrolls)
            {
                if (p.Employee != null && p.Employee.Department == null && allDepts.ContainsKey(p.Employee.DepartmentId))
                {
                    p.Employee.Department = new Department { Name = allDepts[p.Employee.DepartmentId] };
                }
            }

            if (departmentId.HasValue)
                payrolls = payrolls.Where(p => p.Employee.DepartmentId == departmentId.Value);

            payrolls = payrolls.OrderBy(p => p.Employee?.FirstName);

            if (format.ToLower() == "csv")
            {
                var sb = new StringBuilder();
                sb.AppendLine("Month,Year,Employee Code,Employee Name,Basic Salary,Allowances,Bonuses,Overtime Hours,Overtime Rate,Overtime Pay,Gross Salary,Tax Rate %,Tax Deductions,Other Deductions,Net Salary,Status,Payment Date,Payment Method");
                foreach (var p in payrolls)
                {
                    sb.AppendLine($"{p.Month}," +
                                  $"{p.Year}," +
                                  $"{EscapeCsv(p.Employee?.EmployeeCode)}," +
                                  $"{EscapeCsv(p.Employee?.FullName)}," +
                                  $"{p.BasicSalary}," +
                                  $"{p.Allowances}," +
                                  $"{p.Bonuses}," +
                                  $"{p.OvertimeHours}," +
                                  $"{p.OvertimeRatePerHour}," +
                                  $"{p.OvertimePay}," +
                                  $"{p.GrossSalary}," +
                                  $"{p.TaxRate}," +
                                  $"{p.TaxDeductions}," +
                                  $"{p.OtherDeductions}," +
                                  $"{p.NetSalary}," +
                                  $"{EscapeCsv(p.Status)}," +
                                  $"{(p.PaymentDate.HasValue ? p.PaymentDate.Value.ToString("yyyy-MM-dd") : "—")}," +
                                  $"{EscapeCsv(p.PaymentMethod)}");
                }
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                return File(bytes, "text/csv", $"PayrollReport_{year}_{month:D2}.csv");
            }
            else
            {
                var doc = new PayrollReportDocument(payrolls, month, year);
                var pdfBytes = doc.GeneratePdf();
                return File(pdfBytes, "application/pdf", $"PayrollReport_{year}_{month:D2}.pdf");
            }
        }

        private string EscapeCsv(string? value)
        {
            if (value == null) return string.Empty;
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\r") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}
