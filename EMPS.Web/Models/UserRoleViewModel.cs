using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMPS.Web.Models
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
    }

    public class AssignRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a role.")]
        public string SelectedRole { get; set; } = string.Empty;

        public string CurrentRole { get; set; } = string.Empty;
        public List<string> AvailableRoles { get; set; } = new();

        public int? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
    }
}
