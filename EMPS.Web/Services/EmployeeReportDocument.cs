using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using EMPS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace EMPS.Web.Services
{
    public class EmployeeReportDocument : IDocument
    {
        private readonly IEnumerable<Employee> _employees;
        private static readonly CultureInfo Currency = new CultureInfo("en-US");

        public EmployeeReportDocument(IEnumerable<Employee> employees)
        {
            _employees = employees;
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = "Employee Directory Report",
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
                            right.Item().AlignRight().Text("EMPLOYEE REPORT")
                                .FontSize(14).Bold().FontColor("#111827");
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
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(70);  // Code
                            c.RelativeColumn(2);   // Name
                            c.RelativeColumn(2.5f); // Email
                            c.RelativeColumn(1.5f); // Phone
                            c.RelativeColumn(1.2f); // Department
                            c.RelativeColumn(1.2f); // Designation
                            c.ConstantColumn(80);  // Joining Date
                            c.ConstantColumn(60);  // Status
                            c.ConstantColumn(80);  // Basic Salary
                        });

                        table.Header(header =>
                        {
                            void HCell(string text) => header.Cell().Background("#f3f4f6").Padding(6).Text(text).Bold().FontColor("#374151");

                            HCell("Code");
                            HCell("Name");
                            HCell("Email");
                            HCell("Phone");
                            HCell("Department");
                            HCell("Designation");
                            HCell("Joining Date");
                            HCell("Status");
                            header.Cell().Background("#f3f4f6").Padding(6).AlignRight().Text("Basic Salary").Bold().FontColor("#374151");
                        });

                        foreach (var emp in _employees)
                        {
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(emp.EmployeeCode);
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(emp.FullName);
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(emp.Email);
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(emp.PhoneNumber);
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(emp.Department?.Name ?? "—");
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(emp.Designation?.Name ?? "—");
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(emp.JoiningDate.ToString("dd MMM yyyy"));
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(emp.EmploymentStatus);
                            table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).AlignRight().Text(emp.BasicSalary.ToString("C", Currency));
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
