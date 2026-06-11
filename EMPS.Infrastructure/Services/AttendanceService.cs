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
            var attendances = await _unitOfWork.Attendances.GetAllWithIncludesAsync(a => a.Employee);
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

        public async Task<IEnumerable<Attendance>> GetByDateAsync(DateTime date)
        {
            var results = await _unitOfWork.Attendances.FindWithIncludesAsync(
                a => a.Date.Date == date.Date,
                a => a.Employee);
            return results.OrderBy(a => a.Employee.FirstName);
        }

        public async Task<IEnumerable<Attendance>> GetMonthlyReportAsync(int month, int year, int? departmentId = null)
        {
            var results = await _unitOfWork.Attendances.FindWithIncludesAsync(
                a => a.Date.Month == month && a.Date.Year == year,
                a => a.Employee);

            if (departmentId.HasValue)
                results = results.Where(a => a.Employee.DepartmentId == departmentId.Value);

            return results.OrderBy(a => a.Employee.FirstName).ThenBy(a => a.Date);
        }

        public async Task<IEnumerable<Attendance>> GetEmployeeHistoryAsync(int employeeId, DateTime? from = null, DateTime? to = null)
        {
            var results = await _unitOfWork.Attendances.FindWithIncludesAsync(
                a => a.EmployeeId == employeeId,
                a => a.Employee);

            if (from.HasValue)
                results = results.Where(a => a.Date.Date >= from.Value.Date);
            if (to.HasValue)
                results = results.Where(a => a.Date.Date <= to.Value.Date);

            return results.OrderByDescending(a => a.Date);
        }
    }
}
