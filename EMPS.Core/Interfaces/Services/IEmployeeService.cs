using System.Collections.Generic;
using System.Threading.Tasks;
using EMPS.Core.Entities;

namespace EMPS.Core.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Employee?> GetEmployeeByCodeAsync(string code);
        Task CreateEmployeeAsync(Employee employee, string userId);
        Task UpdateEmployeeAsync(Employee employee, string userId);
        Task DeleteEmployeeAsync(int id, string userId);
        Task<string> GenerateEmployeeCodeAsync();
    }
}
