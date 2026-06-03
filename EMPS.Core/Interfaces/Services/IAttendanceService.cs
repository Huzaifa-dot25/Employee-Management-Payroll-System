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
    }
}
