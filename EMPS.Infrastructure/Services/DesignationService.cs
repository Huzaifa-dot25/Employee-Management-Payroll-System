using System;
using System.Collections.Generic;
using System.Linq;
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
            // We want to include Department info, so custom fetching might be needed if generic repository doesn't include it.
            // But since Department is configured, let's fetch designations.
            return await _unitOfWork.Designations.GetAllAsync();
        }

        public async Task<Designation?> GetDesignationByIdAsync(int id)
        {
            return await _unitOfWork.Designations.GetByIdAsync(id);
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
