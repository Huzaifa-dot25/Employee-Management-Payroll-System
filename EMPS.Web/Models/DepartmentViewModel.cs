using System.ComponentModel.DataAnnotations;

namespace EMPS.Web.Models
{
    public class DepartmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Department Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department Code is required")]
        [StringLength(20, ErrorMessage = "Code cannot exceed 20 characters")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public int EmployeeCount { get; set; }
    }
}
