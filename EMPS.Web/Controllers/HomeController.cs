using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using EMPS.Core.Interfaces;
using EMPS.Core.Entities;
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
            var employees = await _unitOfWork.Employees.GetAllAsync();
            var payrolls = await _unitOfWork.Payrolls.GetAllAsync();
            var leaves = await _unitOfWork.LeaveRequests.GetAllAsync();
            var attendances = await _unitOfWork.Attendances.GetAllAsync();

            var today = DateTime.Today;

            var viewModel = new DashboardViewModel
            {
                TotalEmployees = employees.Count(),
                ActiveEmployees = employees.Count(e => e.EmploymentStatus == "Active"),
                TotalSalaryPaid = payrolls.Where(p => p.Status == "Paid").Sum(p => p.NetSalary),
                PendingLeaveRequests = leaves.Count(l => l.Status == "Pending"),
                PresentToday = attendances.Count(a => a.Date.Date == today.Date && a.Status == "Present"),
                
                RecentEmployees = _mapper.Map<IEnumerable<EmployeeViewModel>>(
                    employees.OrderByDescending(e => e.CreatedAt).Take(5)
                ),
                RecentLeaves = leaves.OrderByDescending(l => l.CreatedAt).Take(5),
                
                // Construct Mock Chart Data for last 6 months
                MonthlyLabels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
                MonthlySalaryPayouts = new List<decimal> { 120000, 125000, 130000, 128000, 135000, 140000 }
            };

            // If actual payroll data exists, use it for charts
            var recentPaidPayrolls = payrolls
                .Where(p => p.Status == "Paid")
                .GroupBy(p => new { p.Year, p.Month })
                .OrderByDescending(g => g.Key.Year)
                .ThenByDescending(g => g.Key.Month)
                .Take(6)
                .Reverse()
                .ToList();

            if (recentPaidPayrolls.Any())
            {
                viewModel.MonthlyLabels.Clear();
                viewModel.MonthlySalaryPayouts.Clear();

                foreach (var gp in recentPaidPayrolls)
                {
                    var monthName = new DateTime(gp.Key.Year, gp.Key.Month, 1).ToString("MMM");
                    viewModel.MonthlyLabels.Add($"{monthName} {gp.Key.Year}");
                    viewModel.MonthlySalaryPayouts.Add(gp.Sum(p => p.NetSalary));
                }
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
