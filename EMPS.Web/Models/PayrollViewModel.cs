using System;
using System.ComponentModel.DataAnnotations;

namespace EMPS.Web.Models
{
    public class PayrollViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }
        public string? EmployeeName  { get; set; }
        public string? EmployeeCode  { get; set; }
        public string? DepartmentName  { get; set; }
        public string? DesignationName { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; }

        // ── Earnings ─────────────────────────────────────────────────────────
        [Required]
        [Display(Name = "Basic Salary")]
        [Range(0, double.MaxValue, ErrorMessage = "Must be ≥ 0")]
        public decimal BasicSalary { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Allowances { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Bonuses { get; set; }

        [Display(Name = "Overtime Hours")]
        [Range(0, 999)]
        public decimal OvertimeHours { get; set; }

        [Display(Name = "Overtime Rate / Hour")]
        [Range(0, double.MaxValue)]
        public decimal OvertimeRatePerHour { get; set; }

        // Derived — computed in JS on the form; sent back as hidden fields
        [Display(Name = "Overtime Pay")]
        public decimal OvertimePay => Math.Round(OvertimeHours * OvertimeRatePerHour, 2);

        // ── Deductions ────────────────────────────────────────────────────────
        [Display(Name = "Tax Rate (%)")]
        [Range(0, 100)]
        public decimal TaxRate { get; set; }

        [Display(Name = "Other Deductions")]
        [Range(0, double.MaxValue)]
        public decimal OtherDeductions { get; set; }

        // Derived
        [Display(Name = "Gross Salary")]
        public decimal GrossSalary   => BasicSalary + Allowances + Bonuses + OvertimePay;

        [Display(Name = "Tax Amount")]
        public decimal TaxDeductions => Math.Round(GrossSalary * TaxRate / 100, 2);

        [Display(Name = "Net Salary")]
        public decimal NetSalary     => GrossSalary - TaxDeductions - OtherDeductions;

        // ── Status / Payment ──────────────────────────────────────────────────
        public string Status { get; set; } = "Draft";

        [DataType(DataType.Date)]
        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Payment Method")]
        public string? PaymentMethod { get; set; }

        public string? Notes { get; set; }

        // ── Payslip info ──────────────────────────────────────────────────────
        public bool    HasPayslip  { get; set; }
        public int?    PayslipId   { get; set; }
        public string? PayslipCode { get; set; }
    }
}
