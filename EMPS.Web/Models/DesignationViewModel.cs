using System.ComponentModel.DataAnnotations;

namespace EMPS.Web.Models
{
    public class DesignationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Designation Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        
        public string? DepartmentName { get; set; }

        [Required(ErrorMessage = "Salary Grade is required")]
        [StringLength(50, ErrorMessage = "Salary Grade cannot exceed 50 characters")]
        [Display(Name = "Salary Grade")]
        public string SalaryGrade { get; set; } = string.Empty;

        [Required(ErrorMessage = "Basic Salary is required")]
        [Range(0, 999999999, ErrorMessage = "Basic Salary must be positive")]
        [Display(Name = "Basic Salary")]
        public decimal BasicSalary { get; set; }

        public int EmployeeCount { get; set; }
    }
}
