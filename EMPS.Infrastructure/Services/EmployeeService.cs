using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMPS.Core.Entities;
using EMPS.Core.Interfaces;
using EMPS.Core.Interfaces.Services;

namespace EMPS.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _unitOfWork.Employees.GetAllWithIncludesAsync(
                e => e.Department,
                e => e.Designation);
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _unitOfWork.Employees.GetByIdWithIncludesAsync(id,
                e => e.Department,
                e => e.Designation);
        }

        public async Task<Employee?> GetEmployeeByCodeAsync(string code)
        {
            var results = await _unitOfWork.Employees.FindAsync(
                e => e.EmployeeCode.ToLower() == code.ToLower());
            return results.FirstOrDefault();
        }

        public async Task CreateEmployeeAsync(Employee employee, string userId)
        {
            if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
                employee.EmployeeCode = await GenerateEmployeeCodeAsync();

            await _unitOfWork.Employees.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        public async Task UpdateEmployeeAsync(Employee employee, string userId)
        {
            _unitOfWork.Employees.Update(employee);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        public async Task DeleteEmployeeAsync(int id, string userId)
        {
            var emp = await _unitOfWork.Employees.GetByIdAsync(id);
            if (emp != null)
            {
                _unitOfWork.Employees.Remove(emp);
                await _unitOfWork.SaveChangesAsync(userId);
            }
        }

        public async Task<string> GenerateEmployeeCodeAsync()
        {
            var allEmployees = await _unitOfWork.Employees.GetAllAsync();
            var last = allEmployees
                .Where(e => e.EmployeeCode.StartsWith("EMP-"))
                .OrderByDescending(e => e.EmployeeCode)
                .FirstOrDefault();

            if (last == null) return "EMP-0001";

            string num = last.EmployeeCode.Substring(4);
            return int.TryParse(num, out int n)
                ? $"EMP-{(n + 1):D4}"
                : $"EMP-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }
    }
}
