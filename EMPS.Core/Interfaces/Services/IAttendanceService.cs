using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMPS.Core.Entities;

namespace EMPS.Core.Interfaces.Services
{
    public interface IAttendanceService
    {
        Task<IEnumerable<Attendance>> GetAllAttendancesAsync();
        Task<Attendance?> GetAttendanceByIdAsync(int id);
        Task<IEnumerable<Attendance>> GetAttendancesByEmployeeIdAsync(int employeeId);
        Task CreateAttendanceAsync(Attendance attendance, string userId);
        Task UpdateAttendanceAsync(Attendance attendance, string userId);
        Task DeleteAttendanceAsync(int id, string userId);

        /// <summary>Returns all attendance records for a specific date (Admin/HR).</summary>
        Task<IEnumerable<Attendance>> GetByDateAsync(DateTime date);

        /// <summary>Returns all attendance records for a given month/year, optionally filtered by department.</summary>
        Task<IEnumerable<Attendance>> GetMonthlyReportAsync(int month, int year, int? departmentId = null);

        /// <summary>Returns full attendance history for one employee with optional date-range filter.</summary>
        Task<IEnumerable<Attendance>> GetEmployeeHistoryAsync(int employeeId, DateTime? from = null, DateTime? to = null);
    }
}
