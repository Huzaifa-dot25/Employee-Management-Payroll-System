using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EMPS.Web.Models
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Employee Code")]
        public string? EmployeeCode { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string? FullName => $"{FirstName} {LastName}";

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [StringLength(20, ErrorMessage = "Phone Number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Profile Photo")]
        public string? ProfilePhotoPath { get; set; }

        [Display(Name = "Upload Profile Photo")]
        public IFormFile? ProfileImage { get; set; }

        [Required(ErrorMessage = "Joining Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Joining Date")]
        public DateTime JoiningDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Employment Status is required")]
        [Display(Name = "Employment Status")]
        public string EmploymentStatus { get; set; } = "Active"; // Active, Inactive, Terminated, Resigned

        [Required(ErrorMessage = "Bank Name is required")]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bank Account Number is required")]
        [Display(Name = "Bank Account Number")]
        public string BankAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tax ID/PAN is required")]
        [Display(Name = "Tax Identification/PAN")]
        public string TaxIdentificationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Basic Salary is required")]
        [Range(0, 999999999, ErrorMessage = "Basic Salary must be positive")]
        [Display(Name = "Basic Salary")]
        public decimal BasicSalary { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        
        public string? DepartmentName { get; set; }

        [Required(ErrorMessage = "Designation is required")]
        [Display(Name = "Designation")]
        public int DesignationId { get; set; }
        
        public string? DesignationName { get; set; }
    }
}
