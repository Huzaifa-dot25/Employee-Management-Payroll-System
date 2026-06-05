using System.Collections.Generic;

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

        // Use ViewModel (not entity) so Employee nav data is resolved by AutoMapper
        public IEnumerable<LeaveRequestViewModel> RecentLeaves { get; set; } = new List<LeaveRequestViewModel>();

        public List<string>  MonthlyLabels        { get; set; } = new();
        public List<decimal> MonthlySalaryPayouts { get; set; } = new();
    }
}
