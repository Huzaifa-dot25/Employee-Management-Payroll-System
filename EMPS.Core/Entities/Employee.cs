using System;
using System.Collections.Generic;

namespace EMPS.Core.Entities
{
    public class Employee : BaseEntity
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? ProfilePhotoPath { get; set; }
        public DateTime JoiningDate { get; set; }
        public string EmploymentStatus { get; set; } = "Active"; // Active, Inactive, Terminated, Resigned
        
        // Bank & Tax Details
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string TaxIdentificationNumber { get; set; } = string.Empty;
        
        public decimal BasicSalary { get; set; }

        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; } = null!;

        public int DesignationId { get; set; }
        public virtual Designation Designation { get; set; } = null!;

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
    }
}
