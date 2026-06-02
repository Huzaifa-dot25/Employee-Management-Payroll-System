using System;

namespace EMPS.Core.Entities
{
    public class LeaveRequest : BaseEntity
    {
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;
        public string LeaveType { get; set; } = string.Empty; // Casual, Sick, Annual, Maternity, Paternity
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? Remarks { get; set; }

        public int TotalDays => (EndDate - StartDate).Days + 1;
    }
}
