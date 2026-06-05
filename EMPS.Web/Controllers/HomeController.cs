using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMPS.Core.Interfaces;
using EMPS.Web.Models;

namespace EMPS.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HomeController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            // Load with nav properties so AutoMapper can resolve names
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
                PresentToday         = attendances.Count(a => a.Date.Date == today && a.Status == "Present"),

                RecentEmployees = _mapper.Map<IEnumerable<EmployeeViewModel>>(
                    employees.OrderByDescending(e => e.CreatedAt).Take(5)),

                RecentLeaves = _mapper.Map<IEnumerable<LeaveRequestViewModel>>(recentLeavesList),

                MonthlyLabels        = new List<string>  { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
                MonthlySalaryPayouts = new List<decimal> { 0, 0, 0, 0, 0, 0 }
            };

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
