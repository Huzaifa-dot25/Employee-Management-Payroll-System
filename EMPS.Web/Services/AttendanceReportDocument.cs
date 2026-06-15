using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using EMPS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMPS.Web.Services
{
    public class AttendanceReportDocument : IDocument
    {
        private readonly IEnumerable<Attendance> _attendances;
        private readonly DateTime _fromDate;
        private readonly DateTime _toDate;

        public AttendanceReportDocument(IEnumerable<Attendance> attendances, DateTime fromDate, DateTime toDate)
        {
            _attendances = attendances;
            _fromDate = fromDate;
            _toDate = toDate;
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = "Attendance Report",
            Author = "EMPS",
            CreationDate = DateTimeOffset.UtcNow
        };

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9));

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

                        row.ConstantItem(200).Column(right =>
                        {
                            right.Item().AlignRight().Text("ATTENDANCE REPORT")
                                .FontSize(14).Bold().FontColor("#111827");
                            right.Item().AlignRight()
                                .Text($"Period: {_fromDate:dd MMM yyyy} to {_toDate:dd MMM yyyy}")
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
                    // Statistics Row
                    int totalCount = _attendances.Count();
                    int present = _attendances.Count(a => a.Status == "Present");
                    int absent = _attendances.Count(a => a.Status == "Absent");
                    int leave = _attendances.Count(a => a.Status == "Leave");
                    int late = _attendances.Count(a => a.Status == "Late");
                    int halfDay = _attendances.Count(a => a.Status == "HalfDay");

                    col.Item().Background("#f9fafb").Border(1).BorderColor("#e5e7eb").Padding(10).Row(row =>
                    {
                        void StatCell(string label, int val, string color)
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignCenter().Text(label).FontSize(8).FontColor("#4b5563");
                                c.Item().AlignCenter().Text(val.ToString()).Bold().FontSize(12).FontColor(color);
                            });
                        }

                        StatCell("Total Logs", totalCount, "#111827");
                        StatCell("Present", present, "#059669");
                        StatCell("Absent", absent, "#dc2626");
                        StatCell("Leave", leave, "#2563eb");
                        StatCell("Late", late, "#d97706");
                        StatCell("Half Day", halfDay, "#7c3aed");
                    });

                    col.Item().Height(15);

                    // Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(80);  // Date
                            c.ConstantColumn(70);  // Code
                            c.RelativeColumn(2);   // Employee Name
                            c.RelativeColumn(1.5f); // Department
                            c.ConstantColumn(60);  // Status
                            c.ConstantColumn(60);  // Check In
                            c.ConstantColumn(60);  // Check Out
                            c.RelativeColumn(2);   // Remarks
                        });

                        table.Header(header =>
                        {
                            void HCell(string text) => header.Cell().Background("#f3f4f6").Padding(6).Text(text).Bold().FontColor("#374151");

                            HCell("Date");
                            HCell("Code");
                            HCell("Employee");
                            HCell("Department");
                            HCell("Status");
                            HCell("Check In");
                            HCell("Check Out");
                            HCell("Remarks");
                        });

                        foreach (var att in _attendances)
                        {
                            string statusColor = att.Status switch
                            {
                                "Present" => "#059669",
                                "Absent" => "#dc2626",
                                "Leave" => "#2563eb",
                                "Late" => "#d97706",
                                "HalfDay" => "#7c3aed",
                                _ => "#111827"
                            };

                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(att.Date.ToString("dd MMM yyyy"));
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(att.Employee?.EmployeeCode ?? "—");
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(att.Employee?.FullName ?? "—");
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(att.Employee?.Department?.Name ?? "—");
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(att.Status).Bold().FontColor(statusColor);
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(att.CheckInTime?.ToString(@"hh\:mm") ?? "—");
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(att.CheckOutTime?.ToString(@"hh\:mm") ?? "—");
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(att.Remarks ?? "—");
                        }
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
