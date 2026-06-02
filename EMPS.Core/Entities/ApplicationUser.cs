using Microsoft.AspNetCore.Identity;

namespace EMPS.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int? EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }
    }
}
