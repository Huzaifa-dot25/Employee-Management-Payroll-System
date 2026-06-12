using System;

namespace EMPS.Core.Entities
{
    public class Payroll : BaseEntity
    {
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        public int Month { get; set; } // 1-12
        public int Year  { get; set; }

        // ── Earnings ─────────────────────────────────────────────────────────
        public decimal BasicSalary   { get; set; }
        public decimal Allowances    { get; set; }  // housing, transport, etc.
        public decimal Bonuses       { get; set; }

        // Overtime — stored as hours × rate; OvertimePay is derived
        public decimal OvertimeHours       { get; set; }  // hours worked overtime
        public decimal OvertimeRatePerHour { get; set; }  // rate per hour
        public decimal OvertimePay => Math.Round(OvertimeHours * OvertimeRatePerHour, 2);

        // ── Deductions ────────────────────────────────────────────────────────
        /// <summary>Tax percentage (0–100). TaxDeductions amount is derived.</summary>
        public decimal TaxRate       { get; set; }  // e.g. 15 = 15 %
        public decimal OtherDeductions { get; set; }  // PF, insurance, etc.

        // Derived: tax is applied to gross before other deductions
        public decimal GrossSalary    => BasicSalary + Allowances + Bonuses + OvertimePay;
        public decimal TaxDeductions  => Math.Round(GrossSalary * TaxRate / 100, 2);
        public decimal NetSalary      => GrossSalary - TaxDeductions - OtherDeductions;

        // ── Status ────────────────────────────────────────────────────────────
        public string    Status        { get; set; } = "Draft"; // Draft, Approved, Paid
        public DateTime? PaymentDate   { get; set; }
        public string?   PaymentMethod { get; set; } // Bank Transfer, Cash, Cheque
        public string?   Notes         { get; set; }

        public virtual Payslip? Payslip { get; set; }
    }
}
