using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMPS.Core.Entities;
using EMPS.Core.Interfaces;
using EMPS.Core.Interfaces.Services;

namespace EMPS.Infrastructure.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            // Include Employees so EmployeeCount is available via AutoMapper
            return await _unitOfWork.Departments.GetAllWithIncludesAsync(d => d.Employees);
        }

        public async Task<Department?> GetDepartmentByIdAsync(int id)
        {
            return await _unitOfWork.Departments.GetByIdAsync(id);
        }

        public async Task CreateDepartmentAsync(Department department, string userId)
        {
            await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        public async Task UpdateDepartmentAsync(Department department, string userId)
        {
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        public async Task DeleteDepartmentAsync(int id, string userId)
        {
            var dept = await _unitOfWork.Departments.GetByIdAsync(id);
            if (dept != null)
            {
                _unitOfWork.Departments.Remove(dept);
                await _unitOfWork.SaveChangesAsync(userId);
            }
        }

        public async Task<bool> DepartmentExistsAsync(string code, int? excludeId = null)
        {
            var results = await _unitOfWork.Departments.FindAsync(
                d => d.Code.ToLower() == code.ToLower());
            return excludeId.HasValue
                ? results.Any(d => d.Id != excludeId.Value)
                : results.Any();
        }
    }
}
