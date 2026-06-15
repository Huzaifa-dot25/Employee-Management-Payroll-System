using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using EMPS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EMPS.Web.Services
{
    public class PayrollReportDocument : IDocument
    {
        private readonly IEnumerable<Payroll> _payrolls;
        private readonly int _month;
        private readonly int _year;
        private static readonly CultureInfo Currency = new CultureInfo("en-US");

        public PayrollReportDocument(IEnumerable<Payroll> payrolls, int month, int year)
        {
            _payrolls = payrolls;
            _month = month;
            _year = year;
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = "Payroll Summary Report",
            Author = "EMPS",
            CreationDate = DateTimeOffset.UtcNow
        };

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(8.5f));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("EMPS")
                                .FontSize(20).Bold()
                                .FontColor("#4f46e5");
                            left.Item().Text("Employee Management & Payroll System")
                                .FontSize(9).FontColor("#6b7280");
                        });

                        row.ConstantItem(250).Column(right =>
                        {
                            right.Item().AlignRight().Text("PAYROLL SUMMARY REPORT")
                                .FontSize(14).Bold().FontColor("#111827");
                            right.Item().AlignRight()
                                .Text($"Period: {new DateTime(_year, _month, 1):MMMM yyyy}")
                                .FontSize(8).FontColor("#6b7280");
                            right.Item().AlignRight()
                                .Text($"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC")
                                .FontSize(8).FontColor("#6b7280");
                        });
                    });

                    col.Item().PaddingVertical(6)
                        .LineHorizontal(1.5f).LineColor("#4f46e5");
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    // Statistics Summary
                    decimal totalBasic = _payrolls.Sum(p => p.BasicSalary);
                    decimal totalAllowances = _payrolls.Sum(p => p.Allowances);
                    decimal totalBonuses = _payrolls.Sum(p => p.Bonuses);
                    decimal totalOvertime = _payrolls.Sum(p => p.OvertimePay);
                    decimal totalGross = _payrolls.Sum(p => p.GrossSalary);
                    decimal totalTax = _payrolls.Sum(p => p.TaxDeductions);
                    decimal totalOther = _payrolls.Sum(p => p.OtherDeductions);
                    decimal totalNet = _payrolls.Sum(p => p.NetSalary);

                    col.Item().Background("#f8fafc").Border(1).BorderColor("#cbd5e1").Padding(10).Row(row =>
                    {
                        void StatCell(string label, decimal val, string color)
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignCenter().Text(label).FontSize(7.5f).FontColor("#475569");
                                c.Item().AlignCenter().Text(val.ToString("C", Currency)).Bold().FontSize(10.5f).FontColor(color);
                            });
                        }

                        StatCell("Basic Salary", totalBasic, "#1e293b");
                        StatCell("Allowances", totalAllowances, "#1e293b");
                        StatCell("Bonuses", totalBonuses, "#1e293b");
                        StatCell("Overtime Pay", totalOvertime, "#1e293b");
                        StatCell("Gross Salary", totalGross, "#0f766e");
                        StatCell("Tax Deduct.", totalTax, "#b91c1c");
                        StatCell("Other Deduct.", totalOther, "#b91c1c");
                        StatCell("Net Salary", totalNet, "#4f46e5");
                    });

                    col.Item().Height(15);

                    // Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(50);  // Code
                            c.RelativeColumn(2);   // Name
                            c.RelativeColumn(1.5f); // Department
                            c.ConstantColumn(65);  // Basic
                            c.ConstantColumn(55);  // Allowances
                            c.ConstantColumn(50);  // Bonuses
                            c.ConstantColumn(55);  // Overtime
                            c.ConstantColumn(65);  // Gross
                            c.ConstantColumn(50);  // Tax
                            c.ConstantColumn(55);  // Other Ded
                            c.ConstantColumn(65);  // Net
                            c.ConstantColumn(50);  // Status
                        });

                        table.Header(header =>
                        {
                            void HCell(string text, bool alignRight = false)
                            {
                                var cell = header.Cell().Background("#f1f5f9").Padding(5);
                                if (alignRight)
                                    cell.AlignRight().Text(text).Bold().FontColor("#334155");
                                else
                                    cell.Text(text).Bold().FontColor("#334155");
                            }

                            HCell("Code");
                            HCell("Name");
                            HCell("Dept");
                            HCell("Basic", true);
                            HCell("Allow.", true);
                            HCell("Bonus", true);
                            HCell("O.T.", true);
                            HCell("Gross", true);
                            HCell("Tax", true);
                            HCell("Other Ded.", true);
                            HCell("Net", true);
                            HCell("Status");
                        });

                        foreach (var p in _payrolls)
                        {
                            table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).Text(p.Employee?.EmployeeCode ?? "—");
                            table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).Text(p.Employee?.FullName ?? "—");
                            table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).Text(p.Employee?.Department?.Name ?? "—");
                            table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).AlignRight().Text(p.BasicSalary.ToString("C", Currency));
                            table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).AlignRight().Text(p.Allowances.ToString("C", Currency));
                            table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).AlignRight().Text(p.Bonuses.ToString("C", Currency));
                            table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).AlignRight().Text(p.OvertimePay.ToString("C", Currency));
                            table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).AlignRight().Text(p.GrossSalary.ToString("C", Currency));
                            table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).AlignRight().Text(p.TaxDeductions.ToString("C", Currency));
                            table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).AlignRight().Text(p.OtherDeductions.ToString("C", Currency));
                            table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).AlignRight().Text(p.NetSalary.ToString("C", Currency)).Bold();
                            table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).Text(p.Status);
                        }

                        // Total Row
                        table.Cell().BorderTop(1).BorderColor("#cbd5e1").Padding(5).Text("Total").Bold();
                        table.Cell().BorderTop(1).BorderColor("#cbd5e1").Padding(5).Text("");
                        table.Cell().BorderTop(1).BorderColor("#cbd5e1").Padding(5).Text("");
                        table.Cell().BorderTop(1).BorderColor("#cbd5e1").Padding(5).AlignRight().Text(totalBasic.ToString("C", Currency)).Bold();
                        table.Cell().BorderTop(1).BorderColor("#cbd5e1").Padding(5).AlignRight().Text(totalAllowances.ToString("C", Currency)).Bold();
                        table.Cell().BorderTop(1).BorderColor("#cbd5e1").Padding(5).AlignRight().Text(totalBonuses.ToString("C", Currency)).Bold();
                        table.Cell().BorderTop(1).BorderColor("#cbd5e1").Padding(5).AlignRight().Text(totalOvertime.ToString("C", Currency)).Bold();
                        table.Cell().BorderTop(1).BorderColor("#cbd5e1").Padding(5).AlignRight().Text(totalGross.ToString("C", Currency)).Bold();
                        table.Cell().BorderTop(1).BorderColor("#cbd5e1").Padding(5).AlignRight().Text(totalTax.ToString("C", Currency)).Bold();
                        table.Cell().BorderTop(1).BorderColor("#cbd5e1").Padding(5).AlignRight().Text(totalOther.ToString("C", Currency)).Bold();
                        table.Cell().BorderTop(1).BorderColor("#cbd5e1").Padding(5).AlignRight().Text(totalNet.ToString("C", Currency)).Bold();
                        table.Cell().BorderTop(1).BorderColor("#cbd5e1").Padding(5).Text("");
                    });
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Page ").FontSize(8).FontColor("#9ca3af");
                    x.CurrentPageNumber().FontSize(8).FontColor("#9ca3af");
                    x.Span(" of ").FontSize(8).FontColor("#9ca3af");
                    x.TotalPages().FontSize(8).FontColor("#9ca3af");
                });
            });
        }
    }
}
