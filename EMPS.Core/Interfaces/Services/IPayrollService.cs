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
    }
}
