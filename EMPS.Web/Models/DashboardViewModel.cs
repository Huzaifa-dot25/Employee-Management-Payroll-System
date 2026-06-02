using System.Collections.Generic;
using EMPS.Core.Entities;

namespace EMPS.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public decimal TotalSalaryPaid { get; set; }
        public int PendingLeaveRequests { get; set; }
        public int PresentToday { get; set; }
        
        public IEnumerable<EmployeeViewModel> RecentEmployees { get; set; } = new List<EmployeeViewModel>();
        public IEnumerable<LeaveRequest> RecentLeaves { get; set; } = new List<LeaveRequest>();
        
        // Chart Data Lists
        public List<string> MonthlyLabels { get; set; } = new();
        public List<decimal> MonthlySalaryPayouts { get; set; } = new();
    }
}
