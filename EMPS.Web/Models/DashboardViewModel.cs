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

        // ── Employee Check-in status & stats ─────────────────────────
        public bool CheckedInToday { get; set; }
        public bool CheckedOutToday { get; set; }
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }

        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public int LeaveDays { get; set; }
        public double AvgWorkingHours { get; set; }

        // ── Charts data ──────────────────────────────────────────────
        public List<string> WeeklyWorkingHoursLabels { get; set; } = new();
        public List<double> WeeklyWorkingHoursData { get; set; } = new();

        public List<string> WeeklyAttendanceLabels { get; set; } = new();
        public List<int> WeeklyPresentData { get; set; } = new();
        public List<int> WeeklyAbsentData { get; set; } = new();
        public List<int> WeeklyLateData { get; set; } = new();
        public List<int> WeeklyLeaveData { get; set; } = new();
    }
}
