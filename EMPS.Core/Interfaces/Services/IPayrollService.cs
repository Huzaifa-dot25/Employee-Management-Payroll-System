using System.Collections.Generic;
using System.Threading.Tasks;
using EMPS.Core.Entities;

namespace EMPS.Core.Interfaces.Services
{
    public interface IPayrollService
    {
        Task<IEnumerable<Payroll>> GetAllPayrollsAsync();
        Task<Payroll?> GetPayrollByIdAsync(int id);
        Task<IEnumerable<Payroll>> GetPayrollsByEmployeeIdAsync(int employeeId);
        Task CreatePayrollAsync(Payroll payroll, string userId);
        Task UpdatePayrollAsync(Payroll payroll, string userId);
        Task DeletePayrollAsync(int id, string userId);
        Task GeneratePayslipAsync(int payrollId, string userId);

        /// <summary>
        /// Builds a pre-filled Payroll object from the employee's basic salary
        /// and default rates. Does NOT persist — used for the Generate Salary wizard.
        /// </summary>
        Task<Payroll> GenerateSalaryAsync(int employeeId, int month, int year);
    }
}
