using System;

namespace EMPS.Core.Entities
{
    public class Payroll : BaseEntity
    {
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;
        
        public int Month { get; set; } // 1-12
        public int Year { get; set; }
        
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; } // Sum of allowances
        public decimal Bonuses { get; set; }
        public decimal OvertimePay { get; set; }
        
        public decimal TaxDeductions { get; set; }
        public decimal OtherDeductions { get; set; } // PF, Insurance, etc.
        
        public decimal GrossSalary => BasicSalary + Allowances + Bonuses + OvertimePay;
        public decimal NetSalary => GrossSalary - (TaxDeductions + OtherDeductions);
        
        public string Status { get; set; } = "Draft"; // Draft, Approved, Paid
        public DateTime? PaymentDate { get; set; }
        public string? PaymentMethod { get; set; } // Bank Transfer, Cash, Cheque
        
        public virtual Payslip? Payslip { get; set; }
    }
}
