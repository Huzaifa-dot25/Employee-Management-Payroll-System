using System;

namespace EMPS.Core.Entities
{
    public class Attendance : BaseEntity
    {
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Present"; // Present, Absent, Leave, Late, HalfDay
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string? Remarks { get; set; }
    }
}
