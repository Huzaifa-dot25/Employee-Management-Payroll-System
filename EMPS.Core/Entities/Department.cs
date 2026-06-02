using System.Collections.Generic;

namespace EMPS.Core.Entities
{
    public class Department : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();
    }
}
