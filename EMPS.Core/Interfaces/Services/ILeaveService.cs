using System.Collections.Generic;
using System.Threading.Tasks;
using EMPS.Core.Entities;

namespace EMPS.Core.Interfaces.Services
{
    public interface ILeaveService
    {
        Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestsAsync();
        Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id);
        Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(int employeeId);
        Task CreateLeaveRequestAsync(LeaveRequest leaveRequest, string userId);
        Task UpdateLeaveRequestAsync(LeaveRequest leaveRequest, string userId);
        Task DeleteLeaveRequestAsync(int id, string userId);
    }
}
