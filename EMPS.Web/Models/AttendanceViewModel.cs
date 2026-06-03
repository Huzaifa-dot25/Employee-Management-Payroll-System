using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EMPS.Web.Models
{
    public class AttendanceViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Employee is required")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }
        
        public string? EmployeeName { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        
        [Required]
        public string Status { get; set; } = "Present";
        
        [DataType(DataType.Time)]
        [Display(Name = "Check In Time")]
        public TimeSpan? CheckInTime { get; set; }
        
        [DataType(DataType.Time)]
        [Display(Name = "Check Out Time")]
        public TimeSpan? CheckOutTime { get; set; }
        
        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
