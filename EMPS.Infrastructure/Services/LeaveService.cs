using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMPS.Core.Entities;
using EMPS.Core.Interfaces;
using EMPS.Core.Interfaces.Services;

namespace EMPS.Infrastructure.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LeaveService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestsAsync()
        {
            var leaves = await _unitOfWork.LeaveRequests.GetAllAsync();
            return leaves.OrderByDescending(l => l.StartDate);
        }

        public async Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id)
        {
            return await _unitOfWork.LeaveRequests.GetByIdAsync(id);
        }

        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
        {
            var results = await _unitOfWork.LeaveRequests.FindAsync(l => l.EmployeeId == employeeId);
            return results.OrderByDescending(l => l.StartDate);
        }

        public async Task CreateLeaveRequestAsync(LeaveRequest leaveRequest, string userId)
        {
            await _unitOfWork.LeaveRequests.AddAsync(leaveRequest);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        public async Task UpdateLeaveRequestAsync(LeaveRequest leaveRequest, string userId)
        {
            _unitOfWork.LeaveRequests.Update(leaveRequest);
            await _unitOfWork.SaveChangesAsync(userId);
        }

        public async Task DeleteLeaveRequestAsync(int id, string userId)
        {
            var leaveRequest = await _unitOfWork.LeaveRequests.GetByIdAsync(id);
            if (leaveRequest != null)
            {
                _unitOfWork.LeaveRequests.Remove(leaveRequest);
                await _unitOfWork.SaveChangesAsync(userId);
            }
        }
    }
}
