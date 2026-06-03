using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMPS.Core.Entities;
using EMPS.Core.Interfaces;
using EMPS.Core.Interfaces.Services;

namespace EMPS.Infrastructure.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttendanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Attendance>> GetAllAttendancesAsync()
        {
            var attendances = await _unitOfWork.Attendances.GetAllAsync();
            return attendances.OrderByDescending(a => a.Date);
        }

        public async Task<Attendance?> GetAttendanceByIdAsync(int id)
        {
            return await _unitOfWork.Attendances.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Attendance>> GetAttendancesByEmployeeIdAsync(int employeeId)
        {
            var results = await _unitOfWork.Attendances.FindAsync(a => a.EmployeeId == employeeId);
            return results.OrderByDescending(a => a.Date);
        }

        public async Task CreateAttendanceAsync(Attendance attendance, string userId)
        {
            await _unitOfWork.Attendances.AddAsync(attendance);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        public async Task UpdateAttendanceAsync(Attendance attendance, string userId)
        {
            _unitOfWork.Attendances.Update(attendance);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        public async Task DeleteAttendanceAsync(int id, string userId)
        {
            var attendance = await _unitOfWork.Attendances.GetByIdAsync(id);
            if (attendance != null)
            {
                _unitOfWork.Attendances.Remove(attendance);
                await _unitOfWork.SaveChangesAsync(userId);
            }
        }
    }
}
