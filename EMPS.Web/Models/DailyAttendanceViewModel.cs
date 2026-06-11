using System;
using System.Collections.Generic;
using System.Linq;

namespace EMPS.Web.Models
{
    public class DailyAttendanceViewModel
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public List<AttendanceViewModel> Records { get; set; } = new();

        // Summary counts
        public int PresentCount  => Records.Count(r => r.Status == "Present");
        public int AbsentCount   => Records.Count(r => r.Status == "Absent");
        public int LeaveCount    => Records.Count(r => r.Status == "Leave");
        public int LateCount     => Records.Count(r => r.Status == "Late");
        public int HalfDayCount  => Records.Count(r => r.Status == "HalfDay");
        public int TotalCount    => Records.Count;
    }
}
