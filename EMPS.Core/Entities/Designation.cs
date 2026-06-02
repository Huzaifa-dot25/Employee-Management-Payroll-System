using System.Collections.Generic;

namespace EMPS.Core.Entities
{
    public class Designation : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; } = null!;
        public string SalaryGrade { get; set; } = string.Empty;
        public decimal BasicSalary { get; set; }

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
