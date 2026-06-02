using System.Collections.Generic;
using System.Threading.Tasks;
using EMPS.Core.Entities;

namespace EMPS.Core.Interfaces.Services
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();
        Task<Department?> GetDepartmentByIdAsync(int id);
        Task CreateDepartmentAsync(Department department, string userId);
        Task UpdateDepartmentAsync(Department department, string userId);
        Task DeleteDepartmentAsync(int id, string userId);
        Task<bool> DepartmentExistsAsync(string code, int? excludeId = null);
    }
}
