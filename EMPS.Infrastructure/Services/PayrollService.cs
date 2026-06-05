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

            // Guard: never create duplicate payslip for the same payroll
            if (payroll.Payslip != null) return;

            var payslip = new Payslip
            {
                PayrollId    = payroll.Id,
                GeneratedAt  = DateTime.UtcNow,
                PayslipCode  = GeneratePayslipCode(payroll),
                PdfFilePath  = null
            };

            await _unitOfWork.Payslips.AddAsync(payslip);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static string GeneratePayslipCode(Payroll payroll)
        {
            // Format: PS-{EmpId}-{Year}{Month:D2}-{shortGuid}
            var shortId = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"PS-{payroll.EmployeeId}-{payroll.Year}{payroll.Month:D2}-{shortId}";
        }
    }
}
