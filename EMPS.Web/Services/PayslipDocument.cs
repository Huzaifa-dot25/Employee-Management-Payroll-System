using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using EMPS.Core.Entities;
using System;
using System.Globalization;

namespace EMPS.Web.Services
{
    /// <summary>
    /// QuestPDF document that renders a professional payslip PDF.
    /// </summary>
    public class PayslipDocument : IDocument
    {
        private readonly Payroll _payroll;
        private static readonly CultureInfo Currency = new CultureInfo("en-US");

        public PayslipDocument(Payroll payroll)
        {
            _payroll = payroll;
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title       = $"Payslip - {_payroll.Payslip?.PayslipCode}",
            Author      = "EMPS",
            CreationDate = DateTimeOffset.UtcNow
        };

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                page.Content().Column(col =>
                {
                    // ── Header ──────────────────────────────────────────────
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("EMPS")
                                .FontSize(26).Bold()
                                .FontColor("#4f46e5");
                            left.Item().Text("Employee Management & Payroll System")
                                .FontSize(9).FontColor("#6b7280");
                        });

                        row.ConstantItem(160).Column(right =>
                        {
                            right.Item().AlignRight().Text("PAYSLIP")
                                .FontSize(18).Bold().FontColor("#111827");
                            right.Item().AlignRight()
                                .Text($"{_payroll.Payslip?.PayslipCode ?? "—"}")
                                .FontSize(9).FontColor("#6b7280");
                            right.Item().AlignRight()
                                .Text($"Generated: {DateTime.UtcNow:dd MMM yyyy}")
                                .FontSize(9).FontColor("#6b7280");
                        });
                    });

                    col.Item().PaddingVertical(6)
                        .LineHorizontal(2).LineColor("#4f46e5");

                    // ── Pay Period Banner ────────────────────────────────────
                    col.Item().Background("#f5f3ff").Padding(10).Row(row =>
                    {
                        row.RelativeItem().Text(text =>
                        {
                            text.Span("Pay Period:  ").FontColor("#6b7280");
                            text.Span(new DateTime(_payroll.Year, _payroll.Month, 1).ToString("MMMM yyyy"))
                                .Bold().FontColor("#4f46e5");
                        });
                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span("Status:  ").FontColor("#6b7280");
                            text.Span(_payroll.Status).Bold().FontColor(
                                _payroll.Status == "Paid" ? "#059669" : "#d97706");
                        });
                    });

                    col.Item().Height(12);

                    // ── Employee Info ────────────────────────────────────────
                    col.Item().Text("Employee Details").Bold().FontSize(11).FontColor("#111827");
                    col.Item().Height(4);

                    col.Item().Border(1).BorderColor("#e5e7eb").Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        void InfoCell(string label, string value)
                        {
                            table.Cell().Padding(8).Column(c =>
                            {
                                c.Item().Text(label).FontSize(8).FontColor("#9ca3af");
                                c.Item().Text(value).Bold().FontColor("#111827");
                            });
                        }

                        var emp = _payroll.Employee;
                        InfoCell("Employee Name",   emp?.FullName ?? "—");
                        InfoCell("Employee Code",   emp?.EmployeeCode ?? "—");
                        InfoCell("Department",      emp?.Department?.Name ?? "—");
                        InfoCell("Designation",     emp?.Designation?.Name ?? "—");
                        InfoCell("Email",           emp?.Email ?? "—");
                        InfoCell("Joining Date",    emp?.JoiningDate.ToString("dd MMM yyyy") ?? "—");
                        InfoCell("Bank",            emp?.BankName ?? "—");
                        InfoCell("Payment Method",  _payroll.PaymentMethod ?? "—");
                    });

                    col.Item().Height(16);

                    // ── Earnings & Deductions ────────────────────────────────
                    col.Item().Row(twoCol =>
                    {
                        // Earnings
                        twoCol.RelativeItem().Column(earn =>
                        {
                            earn.Item().Background("#f0fdf4").Padding(8).Text("Earnings")
                                .Bold().FontSize(11).FontColor("#065f46");

                            earn.Item().Border(1).BorderColor("#d1fae5").Table(t =>
                            {
                                t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });

                                void ERow(string label, decimal amount, bool highlight = false)
                                {
                                    t.Cell().Padding(7).PaddingLeft(8)
                                        .Background(highlight ? "#f0fdf4" : Colors.White)
                                        .Text(label).FontColor("#374151");
                                    t.Cell().Padding(7).PaddingRight(8).AlignRight()
                                        .Background(highlight ? "#f0fdf4" : Colors.White)
                                        .Text(Fmt(amount)).FontColor("#059669").Bold();
                                }

                                ERow("Basic Salary",  _payroll.BasicSalary);
                                ERow("Allowances",    _payroll.Allowances);
                                ERow("Bonuses",       _payroll.Bonuses);
                                ERow("Overtime Pay",  _payroll.OvertimePay);

                                // Gross total row
                                t.Cell().BorderTop(1).BorderColor("#d1fae5")
                                    .Padding(8).Text("Gross Salary").Bold().FontColor("#065f46");
                                t.Cell().BorderTop(1).BorderColor("#d1fae5")
                                    .Padding(8).PaddingRight(8).AlignRight()
                                    .Text(Fmt(_payroll.GrossSalary)).Bold().FontColor("#065f46");
                            });
                        });

                        twoCol.ConstantItem(16);

                        // Deductions
                        twoCol.RelativeItem().Column(ded =>
                        {
                            ded.Item().Background("#fff7ed").Padding(8).Text("Deductions")
                                .Bold().FontSize(11).FontColor("#92400e");

                            ded.Item().Border(1).BorderColor("#fde68a").Table(t =>
                            {
                                t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });

                                void DRow(string label, decimal amount)
                                {
                                    t.Cell().Padding(7).PaddingLeft(8).Text(label).FontColor("#374151");
                                    t.Cell().Padding(7).PaddingRight(8).AlignRight()
                                        .Text(Fmt(amount)).FontColor("#dc2626").Bold();
                                }

                                DRow("Tax",              _payroll.TaxDeductions);
                                DRow("Other Deductions", _payroll.OtherDeductions);

                                var totalDed = _payroll.TaxDeductions + _payroll.OtherDeductions;
                                t.Cell().BorderTop(1).BorderColor("#fde68a")
                                    .Padding(8).Text("Total Deductions").Bold().FontColor("#92400e");
                                t.Cell().BorderTop(1).BorderColor("#fde68a")
                                    .Padding(8).PaddingRight(8).AlignRight()
                                    .Text(Fmt(totalDed)).Bold().FontColor("#92400e");
                            });
                        });
                    });

                    col.Item().Height(16);

                    // ── Net Salary Banner ────────────────────────────────────
                    col.Item().Background("#4f46e5").Padding(14).Row(row =>
                    {
                        row.RelativeItem().Text("NET SALARY")
                            .Bold().FontSize(14).FontColor(Colors.White);
                        row.RelativeItem().AlignRight()
                            .Text(Fmt(_payroll.NetSalary))
                            .Bold().FontSize(18).FontColor(Colors.White);
                    });

                    col.Item().Height(16);

                    // ── Payment Info ─────────────────────────────────────────
                    if (_payroll.PaymentDate.HasValue)
                    {
                        col.Item().Background("#f9fafb").Border(1).BorderColor("#e5e7eb")
                            .Padding(10).Row(row =>
                            {
                                row.RelativeItem().Text(text =>
                                {
                                    text.Span("Payment Date:  ").FontColor("#6b7280");
                                    text.Span(_payroll.PaymentDate.Value.ToString("dd MMM yyyy"))
                                        .Bold().FontColor("#111827");
                                });
                                row.RelativeItem().AlignRight().Text(text =>
                                {
                                    text.Span("Payment Method:  ").FontColor("#6b7280");
                                    text.Span(_payroll.PaymentMethod ?? "—").Bold().FontColor("#111827");
                                });
                            });

                        col.Item().Height(16);
                    }

                    // ── Footer ───────────────────────────────────────────────
                    col.Item().LineHorizontal(1).LineColor("#e5e7eb");
                    col.Item().Height(6);
                    col.Item().AlignCenter().Text(text =>
                    {
                        text.Span("This payslip is computer-generated and does not require a signature.  ")
                            .FontSize(8).FontColor("#9ca3af").Italic();
                        text.Span("EMPS © " + DateTime.UtcNow.Year)
                            .FontSize(8).FontColor("#9ca3af");
                    });
                });
            });
        }

        private static string Fmt(decimal amount) =>
            amount.ToString("C", Currency);
    }
}
