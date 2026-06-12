using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMPS.Core.Entities;
using EMPS.Core.Interfaces;
using EMPS.Core.Interfaces.Services;

namespace EMPS.Infrastructure.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IUnitOfWork _unitOfWork;

        // Default overtime rate multiplier: 1.5× hourly rate
        // Assuming standard 22 working days × 8 hours
        private const decimal WorkingHoursPerMonth = 176m;
        private const decimal OvertimeMultiplier   = 1.5m;
        private const decimal DefaultTaxRate       = 15m; // 15 %

        public PayrollService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Payroll>> GetAllPayrollsAsync()
        {
            var payrolls = await _unitOfWork.Payrolls.GetAllWithIncludesAsync(p => p.Employee);
            return payrolls.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month);
        }

        public async Task<Payroll?> GetPayrollByIdAsync(int id)
        {
            return await _unitOfWork.Payrolls.GetByIdWithIncludesAsync(id,
                p => p.Employee!,
                p => p.Payslip!);
        }

        public async Task<IEnumerable<Payroll>> GetPayrollsByEmployeeIdAsync(int employeeId)
        {
            var results = await _unitOfWork.Payrolls.FindAsync(p => p.EmployeeId == employeeId);
            return results.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month);
        }

        public async Task CreatePayrollAsync(Payroll payroll, string userId)
        {
            await _unitOfWork.Payrolls.AddAsync(payroll);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        public async Task UpdatePayrollAsync(Payroll payroll, string userId)
        {
            _unitOfWork.Payrolls.Update(payroll);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        public async Task DeletePayrollAsync(int id, string userId)
        {
            var payroll = await _unitOfWork.Payrolls.GetByIdAsync(id);
            if (payroll != null)
            {
                _unitOfWork.Payrolls.Remove(payroll);
                await _unitOfWork.SaveChangesAsync(userId);
            }
        }

        public async Task GeneratePayslipAsync(int payrollId, string userId)
        {
            var payroll = await GetPayrollByIdAsync(payrollId);
            if (payroll == null || payroll.Status != "Paid") return;

            if (payroll.Payslip != null) return;

            var payslip = new Payslip
            {
                PayrollId   = payroll.Id,
                GeneratedAt = DateTime.UtcNow,
                PayslipCode = GeneratePayslipCode(payroll),
                PdfFilePath = null
            };

            await _unitOfWork.Payslips.AddAsync(payslip);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        /// <summary>
        /// Auto-fills payroll fields based on employee's basic salary.
        /// Overtime rate = BasicSalary / WorkingHoursPerMonth × 1.5
        /// Tax rate      = DefaultTaxRate (15 %)
        /// Does NOT persist — caller decides what to do with the result.
        /// </summary>
        public async Task<Payroll> GenerateSalaryAsync(int employeeId, int month, int year)
        {
            var employees = await _unitOfWork.Employees.FindWithIncludesAsync(
                e => e.Id == employeeId,
                e => e.Department!,
                e => e.Designation!);

            var emp = employees.FirstOrDefault()
                ?? throw new InvalidOperationException($"Employee {employeeId} not found.");

            var hourlyRate    = emp.BasicSalary / WorkingHoursPerMonth;
            var overtimeRate  = Math.Round(hourlyRate * OvertimeMultiplier, 2);

            return new Payroll
            {
                EmployeeId         = employeeId,
                Employee           = emp,
                Month              = month,
                Year               = year,
                BasicSalary        = emp.BasicSalary,
                Allowances         = 0,
                Bonuses            = 0,
                OvertimeHours      = 0,
                OvertimeRatePerHour = overtimeRate,
                TaxRate            = DefaultTaxRate,
                OtherDeductions    = 0,
                Status             = "Draft"
            };
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static string GeneratePayslipCode(Payroll payroll)
        {
            var shortId = Guid.NewGuid().ToString("N")[..6].ToUpper();
            return $"PS-{payroll.EmployeeId}-{payroll.Year}{payroll.Month:D2}-{shortId}";
        }
    }
}
