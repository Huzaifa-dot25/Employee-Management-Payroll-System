using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EMPS.Infrastructure.Data;
using EMPS.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EMPS.Web.Services
{
    public interface IChatAssistantService
    {
        Task<string> ProcessMessageAsync(string message);
    }

    public class ChatAssistantService : IChatAssistantService
    {
        private readonly ApplicationDbContext _context;

        public ChatAssistantService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> ProcessMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return GetGreetingResponse();
            }

            string normalized = message.ToLowerInvariant().Trim();

            try
            {
                // 1. Absent Employees Query
                if (normalized.Contains("absent") || normalized.Contains("attendance") && normalized.Contains("today"))
                {
                    return await GetAbsentTodayResponseAsync();
                }

                // 2. Pending Leaves Query
                if (normalized.Contains("leave") && (normalized.Contains("pending") || normalized.Contains("request")))
                {
                    return await GetPendingLeavesResponseAsync();
                }

                // 3. Highest Salary Query
                if (normalized.Contains("salary") && (normalized.Contains("highest") || normalized.Contains("most") || normalized.Contains("max") || normalized.Contains("earn")))
                {
                    return await GetHighestSalaryResponseAsync();
                }

                // 4. Department Query
                if (normalized.Contains("department") || normalized.Contains("dept"))
                {
                    return await GetDepartmentEmployeesResponseAsync(normalized);
                }

                // 5. Total Employee Count
                if (normalized.Contains("total") || normalized.Contains("count") || normalized.Contains("how many employees"))
                {
                    return await GetEmployeeCountResponseAsync();
                }

                // 6. List Employees
                if (normalized.Contains("list") && normalized.Contains("employee") || normalized.Contains("show") && normalized.Contains("employee"))
                {
                    return await GetEmployeeListResponseAsync();
                }

                // 7. Greetings
                if (normalized.Contains("hi") || normalized.Contains("hello") || normalized.Contains("hey") || normalized.Contains("help") || normalized.Contains("greet"))
                {
                    return GetGreetingResponse();
                }

                // Fallback: Let's search departments anyway in case they just typed a department name
                var fallbackDeptResponse = await GetDepartmentEmployeesResponseAsync(normalized);
                if (!fallbackDeptResponse.Contains("could not find a department"))
                {
                    return fallbackDeptResponse;
                }

                return GetUnknownQueryResponse(message);
            }
            catch (Exception ex)
            {
                return $@"
                    <div class='assistant-response-error'>
                        <div class='d-flex align-items-center gap-2 text-danger mb-2'>
                            <i class='fa-solid fa-circle-exmark'></i>
                            <strong>Error processing request</strong>
                        </div>
                        <p class='text-muted mb-0' style='font-size:0.85rem;'>{ex.Message}</p>
                    </div>";
            }
        }

        private string GetGreetingResponse()
        {
            return @"
                <div class='assistant-response-card'>
                    <div class='assistant-welcome-header mb-3'>
                        <div class='welcome-icon'><i class='fa-solid fa-robot'></i></div>
                        <div>
                            <h6 class='mb-0 fw-700 text-main'>HR AI Assistant</h6>
                            <small class='text-muted'>Ready to read the database</small>
                        </div>
                    </div>
                    <p class='mb-3 text-sub' style='font-size: 0.9rem;'>
                        Hello! I am your HR Chat Assistant. I can query the Employee Database directly to retrieve real-time payroll, attendance, and department stats.
                    </p>
                    <div class='suggestion-prompt-label mb-2'>Try asking me:</div>
                    <div class='suggested-queries-list d-flex flex-column gap-2'>
                        <button class='suggest-btn' onclick='sendQuickMessage(""How many employees are absent today?"")'>
                            <i class='fa-solid fa-calendar-minus me-2 text-warning'></i>How many employees are absent today?
                        </button>
                        <button class='suggest-btn' onclick='sendQuickMessage(""Show employees in the IT department."")'>
                            <i class='fa-solid fa-sitemap me-2 text-primary'></i>Show employees in the IT department.
                        </button>
                        <button class='suggest-btn' onclick='sendQuickMessage(""Who has the highest salary?"")'>
                            <i class='fa-solid fa-money-bill-wave me-2 text-success'></i>Who has the highest salary?
                        </button>
                        <button class='suggest-btn' onclick='sendQuickMessage(""How many leave requests are pending?"")'>
                            <i class='fa-solid fa-business-time me-2 text-info'></i>How many leave requests are pending?
                        </button>
                    </div>
                </div>";
        }

        private async Task<string> GetAbsentTodayResponseAsync()
        {
            var today = DateTime.Today;
            var absentees = await _context.Attendances
                .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
                .Where(a => a.Date.Date == today && a.Status == "Absent" && !a.IsDeleted)
                .ToListAsync();

            if (!absentees.Any())
            {
                return @"
                    <div class='assistant-response-card'>
                        <div class='d-flex align-items-center gap-2 mb-3'>
                            <div class='stat-indicator bg-success-light text-success'><i class='fa-solid fa-circle-check'></i></div>
                            <h6 class='mb-0 fw-700 text-main'>Today's Absences</h6>
                        </div>
                        <div class='alert alert-success border-0 py-2 px-3 mb-0' role='alert'>
                            <i class='fa-solid fa-square-poll-horizontal me-2'></i>All employees are present or have not been marked absent today.
                        </div>
                    </div>";
            }

            var rows = string.Join("", absentees.Select(a => $@"
                <tr>
                    <td>
                        <div class='fw-600 text-main'>{a.Employee.FullName}</div>
                        <small class='text-muted'>{a.Employee.EmployeeCode}</small>
                    </td>
                    <td><span class='badge bg-light text-dark border'>{a.Employee.Department?.Name ?? "N/A"}</span></td>
                    <td><span class='badge bg-danger-light text-danger fw-700'>{a.Status}</span></td>
                </tr>"));

            return $@"
                <div class='assistant-response-card'>
                    <div class='d-flex align-items-center justify-content-between mb-3'>
                        <div class='d-flex align-items-center gap-2'>
                            <div class='stat-indicator bg-danger-light text-danger'><i class='fa-solid fa-calendar-minus'></i></div>
                            <h6 class='mb-0 fw-700 text-main'>Today's Absences</h6>
                        </div>
                        <span class='badge bg-danger rounded-pill px-2.5 py-1 fw-800'>{absentees.Count} Absent</span>
                    </div>
                    <div class='table-responsive border rounded'>
                        <table class='table table-sm table-custom mb-0'>
                            <thead>
                                <tr>
                                    <th>Employee</th>
                                    <th>Department</th>
                                    <th>Status</th>
                                </tr>
                            </thead>
                            <tbody>
                                {rows}
                            </tbody>
                        </table>
                    </div>
                </div>";
        }

        private async Task<string> GetPendingLeavesResponseAsync()
        {
            var pendingLeaves = await _context.LeaveRequests
                .Include(l => l.Employee)
                .ThenInclude(e => e.Department)
                .Where(l => l.Status == "Pending" && !l.IsDeleted)
                .OrderBy(l => l.StartDate)
                .ToListAsync();

            if (!pendingLeaves.Any())
            {
                return @"
                    <div class='assistant-response-card'>
                        <div class='d-flex align-items-center gap-2 mb-3'>
                            <div class='stat-indicator bg-success-light text-success'><i class='fa-solid fa-circle-check'></i></div>
                            <h6 class='mb-0 fw-700 text-main'>Pending Leave Requests</h6>
                        </div>
                        <div class='alert alert-success border-0 py-2 px-3 mb-0' role='alert'>
                            <i class='fa-solid fa-circle-check me-2'></i>No pending leave requests found.
                        </div>
                    </div>";
            }

            var rows = string.Join("", pendingLeaves.Select(l => $@"
                <tr>
                    <td>
                        <div class='fw-600 text-main'>{l.Employee.FullName}</div>
                        <small class='text-muted'>{l.Employee.Department?.Name ?? "N/A"}</small>
                    </td>
                    <td><span class='badge bg-info-light text-info fw-700'>{l.LeaveType}</span></td>
                    <td>
                        <div style='font-size:0.8rem;'>{l.StartDate:MMM dd} - {l.EndDate:MMM dd}</div>
                        <small class='text-muted'>{l.TotalDays} day(s)</small>
                    </td>
                    <td class='text-truncate' style='max-width: 150px;' title='{l.Reason}'>{l.Reason}</td>
                </tr>"));

            return $@"
                <div class='assistant-response-card'>
                    <div class='d-flex align-items-center justify-content-between mb-3'>
                        <div class='d-flex align-items-center gap-2'>
                            <div class='stat-indicator bg-info-light text-info'><i class='fa-solid fa-business-time'></i></div>
                            <h6 class='mb-0 fw-700 text-main'>Pending Leaves</h6>
                        </div>
                        <span class='badge bg-info rounded-pill px-2.5 py-1 fw-800'>{pendingLeaves.Count} Pending</span>
                    </div>
                    <div class='table-responsive border rounded'>
                        <table class='table table-sm table-custom mb-0'>
                            <thead>
                                <tr>
                                    <th>Employee</th>
                                    <th>Type</th>
                                    <th>Duration</th>
                                    <th>Reason</th>
                                </tr>
                            </thead>
                            <tbody>
                                {rows}
                            </tbody>
                        </table>
                    </div>
                </div>";
        }

        private async Task<string> GetHighestSalaryResponseAsync()
        {
            var maxSalary = await _context.Employees
                .Where(e => !e.IsDeleted && e.EmploymentStatus == "Active")
                .MaxAsync(e => (decimal?)e.BasicSalary) ?? 0;

            if (maxSalary == 0)
            {
                return @"
                    <div class='assistant-response-card'>
                        <p class='text-muted mb-0'>No active employees or salaries found in the database.</p>
                    </div>";
            }

            var topEarners = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Where(e => e.BasicSalary == maxSalary && !e.IsDeleted && e.EmploymentStatus == "Active")
                .ToListAsync();

            var cards = string.Join("", topEarners.Select(e => $@"
                <div class='d-flex align-items-center gap-3 p-3 border rounded mb-2 bg-light shadow-sm'>
                    <div class='user-avatar' style='width:48px; height:48px; font-size:1.1rem;'>
                        {e.FullName.Substring(0, 1).ToUpper()}
                    </div>
                    <div class='flex-grow-1'>
                        <h6 class='mb-0 fw-700 text-main'>{e.FullName}</h6>
                        <small class='text-muted'>{e.Designation?.Name} | {e.Department?.Name}</small>
                        <div class='mt-1 fw-800 text-success' style='font-size:1.05rem;'>${e.BasicSalary:N2}</div>
                    </div>
                </div>"));

            return $@"
                <div class='assistant-response-card'>
                    <div class='d-flex align-items-center gap-2 mb-3'>
                        <div class='stat-indicator bg-success-light text-success'><i class='fa-solid fa-money-bill-wave'></i></div>
                        <h6 class='mb-0 fw-700 text-main'>Highest Earning Employee(s)</h6>
                    </div>
                    {cards}
                </div>";
        }

        private async Task<string> GetDepartmentEmployeesResponseAsync(string message)
        {
            var departments = await _context.Departments.Where(d => !d.IsDeleted).ToListAsync();
            Department? targetDept = null;

            foreach (var dept in departments)
            {
                if (message.Contains(dept.Name.ToLowerInvariant()) || message.Contains(dept.Code.ToLowerInvariant()))
                {
                    targetDept = dept;
                    break;
                }
            }

            // Fallback keywords (e.g. "it" might match IT code, or "information technology")
            if (targetDept == null)
            {
                if (message.Contains(" it ") || message.Contains("it department") || message.StartsWith("it ") || message.EndsWith(" it") || message.Equals("it"))
                {
                    targetDept = departments.FirstOrDefault(d => d.Code.Equals("IT", StringComparison.OrdinalIgnoreCase) || d.Name.Contains("IT", StringComparison.OrdinalIgnoreCase));
                }
                else if (message.Contains("hr") || message.Contains("human resources"))
                {
                    targetDept = departments.FirstOrDefault(d => d.Code.Equals("HR", StringComparison.OrdinalIgnoreCase) || d.Name.Contains("HR", StringComparison.OrdinalIgnoreCase) || d.Name.Contains("Human Resources", StringComparison.OrdinalIgnoreCase));
                }
                else if (message.Contains("finance"))
                {
                    targetDept = departments.FirstOrDefault(d => d.Name.Contains("Finance", StringComparison.OrdinalIgnoreCase));
                }
            }

            if (targetDept == null)
            {
                var deptChips = string.Join(" ", departments.Select(d => $@"
                    <button class='badge bg-primary border-0 me-1 py-1.5 px-2.5 text-white' onclick='sendQuickMessage(""Show employees in the {d.Name} department."")'>{d.Name}</button>"));

                return $@"
                    <div class='assistant-response-card'>
                        <div class='d-flex align-items-center gap-2 mb-2'>
                            <i class='fa-solid fa-circle-question text-warning'></i>
                            <span class='fw-700 text-main'>Which department?</span>
                        </div>
                        <p class='text-muted' style='font-size:0.875rem;'>I could not find a department matching your input. Click one below to query:</p>
                        <div class='d-flex flex-wrap gap-1.5 mt-2'>
                            {deptChips}
                        </div>
                    </div>";
            }

            var employees = await _context.Employees
                .Include(e => e.Designation)
                .Where(e => e.DepartmentId == targetDept.Id && e.EmploymentStatus == "Active" && !e.IsDeleted)
                .ToListAsync();

            if (!employees.Any())
            {
                return $@"
                    <div class='assistant-response-card'>
                        <div class='d-flex align-items-center gap-2 mb-3'>
                            <div class='stat-indicator bg-primary-light text-primary'><i class='fa-solid fa-sitemap'></i></div>
                            <h6 class='mb-0 fw-700 text-main'>{targetDept.Name} Department</h6>
                        </div>
                        <p class='text-muted mb-0'>No active employees are currently assigned to the {targetDept.Name} department.</p>
                    </div>";
            }

            var rows = string.Join("", employees.Select(e => $@"
                <tr>
                    <td>
                        <div class='fw-600 text-main'>{e.FullName}</div>
                        <small class='text-muted'>{e.EmployeeCode}</small>
                    </td>
                    <td>{e.Designation?.Name ?? "N/A"}</td>
                    <td>{e.Email}</td>
                    <td class='fw-600 text-success'>${e.BasicSalary:N0}</td>
                </tr>"));

            return $@"
                <div class='assistant-response-card'>
                    <div class='d-flex align-items-center justify-content-between mb-3'>
                        <div class='d-flex align-items-center gap-2'>
                            <div class='stat-indicator bg-primary-light text-primary'><i class='fa-solid fa-sitemap'></i></div>
                            <h6 class='mb-0 fw-700 text-main'>{targetDept.Name} Employees</h6>
                        </div>
                        <span class='badge bg-primary rounded-pill px-2.5 py-1 fw-800'>{employees.Count} Active</span>
                    </div>
                    <div class='table-responsive border rounded'>
                        <table class='table table-sm table-custom mb-0'>
                            <thead>
                                <tr>
                                    <th>Employee</th>
                                    <th>Designation</th>
                                    <th>Email</th>
                                    <th>Salary</th>
                                </tr>
                            </thead>
                            <tbody>
                                {rows}
                            </tbody>
                        </table>
                    </div>
                </div>";
        }

        private async Task<string> GetEmployeeCountResponseAsync()
        {
            var employees = await _context.Employees.Where(e => !e.IsDeleted).ToListAsync();
            var total = employees.Count;
            var active = employees.Count(e => e.EmploymentStatus == "Active");
            var inactive = employees.Count(e => e.EmploymentStatus == "Inactive");
            var terminated = employees.Count(e => e.EmploymentStatus == "Terminated" || e.EmploymentStatus == "Resigned");

            return $@"
                <div class='assistant-response-card'>
                    <div class='d-flex align-items-center gap-2 mb-3'>
                        <div class='stat-indicator bg-primary-light text-primary'><i class='fa-solid fa-users'></i></div>
                        <h6 class='mb-0 fw-700 text-main'>Employee Database Stats</h6>
                    </div>
                    <div class='row g-2 mb-3'>
                        <div class='col-6'>
                            <div class='p-2.5 border rounded text-center bg-light'>
                                <div class='text-muted' style='font-size:0.75rem; font-weight:700; text-transform:uppercase;'>Total registered</div>
                                <div class='fw-800 text-main' style='font-size:1.4rem;'>{total}</div>
                            </div>
                        </div>
                        <div class='col-6'>
                            <div class='p-2.5 border rounded text-center bg-success-light'>
                                <div class='text-success' style='font-size:0.75rem; font-weight:700; text-transform:uppercase;'>Active Status</div>
                                <div class='fw-800 text-success' style='font-size:1.4rem;'>{active}</div>
                            </div>
                        </div>
                    </div>
                    <div class='d-flex justify-content-between text-muted px-1' style='font-size:0.8rem;'>
                        <span>Inactive: <strong>{inactive}</strong></span>
                        <span>Resigned/Terminated: <strong>{terminated}</strong></span>
                    </div>
                </div>";
        }

        private async Task<string> GetEmployeeListResponseAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Where(e => !e.IsDeleted && e.EmploymentStatus == "Active")
                .OrderBy(e => e.FirstName)
                .Take(8)
                .ToListAsync();

            var totalCount = await _context.Employees.CountAsync(e => !e.IsDeleted && e.EmploymentStatus == "Active");

            var rows = string.Join("", employees.Select(e => $@"
                <tr>
                    <td>
                        <div class='fw-600 text-main'>{e.FullName}</div>
                        <small class='text-muted'>{e.EmployeeCode}</small>
                    </td>
                    <td>{e.Department?.Name ?? "N/A"}</td>
                    <td>{e.Designation?.Name ?? "N/A"}</td>
                </tr>"));

            return $@"
                <div class='assistant-response-card'>
                    <div class='d-flex align-items-center justify-content-between mb-3'>
                        <div class='d-flex align-items-center gap-2'>
                            <div class='stat-indicator bg-primary-light text-primary'><i class='fa-solid fa-users'></i></div>
                            <h6 class='mb-0 fw-700 text-main'>Active Employees</h6>
                        </div>
                        <span class='badge bg-light text-dark border px-2 py-1' style='font-size:0.75rem;'>Showing 8 of {totalCount}</span>
                    </div>
                    <div class='table-responsive border rounded mb-2'>
                        <table class='table table-sm table-custom mb-0'>
                            <thead>
                                <tr>
                                    <th>Employee</th>
                                    <th>Department</th>
                                    <th>Designation</th>
                                </tr>
                            </thead>
                            <tbody>
                                {rows}
                            </tbody>
                        </table>
                    </div>
                    <p class='text-muted mb-0' style='font-size:0.75rem; font-style:italic;'>For a complete list of employees, please visit the <a href='/Employee' class='text-primary fw-600'>Employees Management</a> page.</p>
                </div>";
        }

        private string GetUnknownQueryResponse(string query)
        {
            return $@"
                <div class='assistant-response-card'>
                    <div class='d-flex align-items-center gap-2 mb-2 text-warning'>
                        <i class='fa-solid fa-triangle-exclamation'></i>
                        <span class='fw-700 text-main'>Query not recognized</span>
                    </div>
                    <p class='text-sub mb-3' style='font-size:0.875rem;'>
                        I couldn't match ""<strong>{HtmlEncode(query)}</strong>"" to a database query. Try asking one of the following:
                    </p>
                    <div class='suggested-queries-list d-flex flex-column gap-2'>
                        <button class='suggest-btn' onclick='sendQuickMessage(""How many employees are absent today?"")'>
                            <i class='fa-solid fa-calendar-minus me-2 text-warning'></i>How many employees are absent today?
                        </button>
                        <button class='suggest-btn' onclick='sendQuickMessage(""Show employees in the IT department."")'>
                            <i class='fa-solid fa-sitemap me-2 text-primary'></i>Show employees in the IT department.
                        </button>
                        <button class='suggest-btn' onclick='sendQuickMessage(""Who has the highest salary?"")'>
                            <i class='fa-solid fa-money-bill-wave me-2 text-success'></i>Who has the highest salary?
                        </button>
                        <button class='suggest-btn' onclick='sendQuickMessage(""How many leave requests are pending?"")'>
                            <i class='fa-solid fa-business-time me-2 text-info'></i>How many leave requests are pending?
                        </button>
                    </div>
                </div>";
        }

        private string HtmlEncode(string input)
        {
            return System.Net.WebUtility.HtmlEncode(input);
        }
    }
}
