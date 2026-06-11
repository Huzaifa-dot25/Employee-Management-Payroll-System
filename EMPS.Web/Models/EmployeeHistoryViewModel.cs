using System;
using System.Collections.Generic;
using System.Linq;

namespace EMPS.Web.Models
{
    public class EmployeeHistoryViewModel
    {
        public int    EmployeeId   { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Department   { get; set; } = string.Empty;
        public string Designation  { get; set; } = string.Empty;

        public DateTime? From { get; set; }
        public DateTime? To   { get; set; }

        public List<AttendanceViewModel> Records { get; set; } = new();

        // Summary
        public int PresentCount => Records.Count(r => r.Status == "Present");
        public int AbsentCount  => Records.Count(r => r.Status == "Absent");
        public int LeaveCount   => Records.Count(r => r.Status == "Leave");
        public int LateCount    => Records.Count(r => r.Status == "Late");
        public int HalfDayCount => Records.Count(r => r.Status == "HalfDay");
        public int TotalCount   => Records.Count;

        public double AttendanceRate =>
            TotalCount == 0 ? 0 : Math.Round((double)PresentCount / TotalCount * 100, 1);
    }
}
