using System;
using System.Collections.Generic;
using System.Linq;

namespace EMPS.Web.Models
{
    /// <summary>Per-employee row in the monthly summary grid.</summary>
    public class EmployeeMonthlyRow
    {
        public int    EmployeeId   { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Department   { get; set; } = string.Empty;

        public int PresentDays  { get; set; }
        public int AbsentDays   { get; set; }
        public int LeaveDays    { get; set; }
        public int LateDays     { get; set; }
        public int HalfDays     { get; set; }
        public int TotalRecorded { get; set; }

        public double AttendanceRate =>
            TotalRecorded == 0 ? 0 : Math.Round((double)PresentDays / TotalRecorded * 100, 1);
    }

    public class MonthlyReportViewModel
    {
        public int  Month        { get; set; } = DateTime.Today.Month;
        public int  Year         { get; set; } = DateTime.Today.Year;
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = "All Departments";

        public List<EmployeeMonthlyRow> Rows { get; set; } = new();

        // Totals
        public int TotalPresent  => Rows.Sum(r => r.PresentDays);
        public int TotalAbsent   => Rows.Sum(r => r.AbsentDays);
        public int TotalLeave    => Rows.Sum(r => r.LeaveDays);
        public int TotalLate     => Rows.Sum(r => r.LateDays);
    }
}
