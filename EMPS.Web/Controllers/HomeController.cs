using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EMPS.Core.Entities;
using EMPS.Core.Interfaces;
using EMPS.Web.Models;

namespace EMPS.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public HomeController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var employees   = await _unitOfWork.Employees.GetAllWithIncludesAsync(e => e.Department, e => e.Designation);
            var payrolls    = await _unitOfWork.Payrolls.GetAllAsync();
            var leaves      = await _unitOfWork.LeaveRequests.GetAllWithIncludesAsync(l => l.Employee);
            var attendances = await _unitOfWork.Attendances.GetAllAsync();

            var today = DateTime.Today;

            var recentLeavesList = leaves
                .OrderByDescending(l => l.CreatedAt)
                .Take(5)
                .ToList();

            var viewModel = new DashboardViewModel
            {
                TotalEmployees       = employees.Count(),
                ActiveEmployees      = employees.Count(e => e.EmploymentStatus == "Active"),
                TotalSalaryPaid      = payrolls.Where(p => p.Status == "Paid").Sum(p => p.NetSalary),
                PendingLeaveRequests = leaves.Count(l => l.Status == "Pending"),
                PresentToday         = attendances.Count(a => a.Date.Date == today && (a.Status == "Present" || a.Status == "Late" || a.Status == "HalfDay")),

                RecentEmployees = _mapper.Map<IEnumerable<EmployeeViewModel>>(
                    employees.OrderByDescending(e => e.CreatedAt).Take(5)),

                RecentLeaves = _mapper.Map<IEnumerable<LeaveRequestViewModel>>(recentLeavesList),

                MonthlyLabels        = new List<string>  { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
                MonthlySalaryPayouts = new List<decimal> { 0, 0, 0, 0, 0, 0 }
            };

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? employeeId = null;
            if (!string.IsNullOrEmpty(userId))
            {
                var appUser = await _userManager.FindByIdAsync(userId);
                employeeId = appUser?.EmployeeId;
            }

            if (User.IsInRole("Employee") && employeeId.HasValue)
            {
                var empId = employeeId.Value;
                var empAttendances = attendances.Where(a => a.EmployeeId == empId).ToList();
                var empLeaves = leaves.Where(l => l.EmployeeId == empId).ToList();
                var todayRecord = empAttendances.FirstOrDefault(a => a.Date.Date == today);

                viewModel.CheckedInToday = todayRecord != null;
                viewModel.CheckedOutToday = todayRecord?.CheckOutTime != null;
                viewModel.CheckInTime = todayRecord?.CheckInTime?.ToString(@"hh\:mm");
                viewModel.CheckOutTime = todayRecord?.CheckOutTime?.ToString(@"hh\:mm");

                viewModel.PresentDays = empAttendances.Count(a => a.Status == "Present");
                viewModel.AbsentDays = empAttendances.Count(a => a.Status == "Absent");
                viewModel.LateDays = empAttendances.Count(a => a.Status == "Late");
                viewModel.LeaveDays = empAttendances.Count(a => a.Status == "Leave");

                var presentWithTime = empAttendances.Where(a => a.CheckInTime.HasValue && a.CheckOutTime.HasValue).ToList();
                viewModel.AvgWorkingHours = presentWithTime.Any() 
                    ? Math.Round(presentWithTime.Average(a => (a.CheckOutTime!.Value - a.CheckInTime!.Value).TotalHours), 1) 
                    : 0;

                // Load last 7 days working hours for the chart
                for (int i = 6; i >= 0; i--)
                {
                    var d = today.AddDays(-i);
                    viewModel.WeeklyWorkingHoursLabels.Add(d.ToString("ddd d"));
                    var record = empAttendances.FirstOrDefault(a => a.Date.Date == d.Date);
                    if (record?.CheckInTime != null && record?.CheckOutTime != null)
                    {
                        viewModel.WeeklyWorkingHoursData.Add(Math.Round((record.CheckOutTime.Value - record.CheckInTime.Value).TotalHours, 1));
                    }
                    else
                    {
                        viewModel.WeeklyWorkingHoursData.Add(0);
                    }
                }

                // Limit recent lists for the employee
                viewModel.RecentLeaves = _mapper.Map<IEnumerable<LeaveRequestViewModel>>(
                    empLeaves.OrderByDescending(l => l.CreatedAt).Take(5));
            }
            else
            {
                // For Admin/HR: Daily/Weekly attendance trend (last 7 days)
                for (int i = 6; i >= 0; i--)
                {
                    var d = today.AddDays(-i);
                    viewModel.WeeklyAttendanceLabels.Add(d.ToString("ddd d"));
                    var dayRecords = attendances.Where(a => a.Date.Date == d.Date).ToList();
                    viewModel.WeeklyPresentData.Add(dayRecords.Count(a => a.Status == "Present"));
                    viewModel.WeeklyAbsentData.Add(dayRecords.Count(a => a.Status == "Absent"));
                    viewModel.WeeklyLateData.Add(dayRecords.Count(a => a.Status == "Late"));
                    viewModel.WeeklyLeaveData.Add(dayRecords.Count(a => a.Status == "Leave"));
                }
            }

            // Replace mock chart data with real data when paid payrolls exist
            var recentPaid = payrolls
                .Where(p => p.Status == "Paid")
                .GroupBy(p => new { p.Year, p.Month })
                .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
                .Take(6).Reverse().ToList();

            if (recentPaid.Any())
            {
                viewModel.MonthlyLabels.Clear();
                viewModel.MonthlySalaryPayouts.Clear();
                foreach (var gp in recentPaid)
                {
                    viewModel.MonthlyLabels.Add(
                        new DateTime(gp.Key.Year, gp.Key.Month, 1).ToString("MMM yy"));
                    viewModel.MonthlySalaryPayouts.Add(gp.Sum(p => p.NetSalary));
                }
            }

            return View(viewModel);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
