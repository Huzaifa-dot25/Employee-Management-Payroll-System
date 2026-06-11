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
        
        public string? EmployeeName { get; set; }
        
        [Required]
        [Range(1, 12)]
        public int Month { get; set; }
        
        [Required]
        public int Year { get; set; }
        
        [Required]
        [Display(Name = "Basic Salary")]
        public decimal BasicSalary { get; set; }
        
        public decimal Allowances { get; set; }
        
        public decimal Bonuses { get; set; }
        
        [Display(Name = "Overtime Pay")]
        public decimal OvertimePay { get; set; }
        
        [Display(Name = "Tax Deductions")]
        public decimal TaxDeductions { get; set; }
        
        [Display(Name = "Other Deductions")]
        public decimal OtherDeductions { get; set; }
        
        [Display(Name = "Gross Salary")]
        public decimal GrossSalary => BasicSalary + Allowances + Bonuses + OvertimePay;
        
        [Display(Name = "Net Salary")]
        public decimal NetSalary => GrossSalary - (TaxDeductions + OtherDeductions);
        
        public string Status { get; set; } = "Draft";
        
        [DataType(DataType.Date)]
        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }
        
        [Display(Name = "Payment Method")]
        public string? PaymentMethod { get; set; }

        // Payslip info (populated from Payroll.Payslip navigation)
        public bool HasPayslip { get; set; }
        public int? PayslipId { get; set; }
        public string? PayslipCode { get; set; }
    }
}
