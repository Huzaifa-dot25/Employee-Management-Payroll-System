using System.Collections.Generic;
using System.Threading.Tasks;
using EMPS.Core.Entities;
using EMPS.Core.Interfaces;
using EMPS.Core.Interfaces.Services;

namespace EMPS.Infrastructure.Services
{
    public class DesignationService : IDesignationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DesignationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Designation>> GetAllDesignationsAsync()
        {
            // Include Department so DepartmentName is available for display
            return await _unitOfWork.Designations.GetAllWithIncludesAsync(
                d => d.Department,
                d => d.Employees);
        }

        public async Task<Designation?> GetDesignationByIdAsync(int id)
        {
            return await _unitOfWork.Designations.GetByIdWithIncludesAsync(id,
                d => d.Department);
        }

        public async Task CreateDesignationAsync(Designation designation, string userId)
        {
            await _unitOfWork.Designations.AddAsync(designation);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        public async Task UpdateDesignationAsync(Designation designation, string userId)
        {
            _unitOfWork.Designations.Update(designation);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        public async Task DeleteDesignationAsync(int id, string userId)
        {
            var designation = await _unitOfWork.Designations.GetByIdAsync(id);
            if (designation != null)
            {
                _unitOfWork.Designations.Remove(designation);
                await _unitOfWork.SaveChangesAsync(userId);
            }
        }

        public async Task<IEnumerable<Designation>> GetDesignationsByDepartmentAsync(int departmentId)
        {
            return await _unitOfWork.Designations.FindAsync(d => d.DepartmentId == departmentId);
        }
    }
}
