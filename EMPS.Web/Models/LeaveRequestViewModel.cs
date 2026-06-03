using System;
using System.ComponentModel.DataAnnotations;

namespace EMPS.Web.Models
{
    public class LeaveRequestViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }
        
        public string? EmployeeName { get; set; }
        
        [Required]
        [Display(Name = "Leave Type")]
        public string LeaveType { get; set; } = string.Empty;
        
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;
        
        public string Status { get; set; } = "Pending";
        
        public string? ApprovedBy { get; set; }
        
        public DateTime? ApprovedAt { get; set; }
        
        public string? Remarks { get; set; }
        
        [Display(Name = "Total Days")]
        public int TotalDays => (EndDate - StartDate).Days + 1;
    }
}
