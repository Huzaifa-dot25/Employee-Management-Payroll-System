using System;

namespace EMPS.Core.Entities
{
    public class Payslip : BaseEntity
    {
        public int PayrollId { get; set; }
        public virtual Payroll Payroll { get; set; } = null!;
        public string PayslipCode { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string? PdfFilePath { get; set; }
    }
}
