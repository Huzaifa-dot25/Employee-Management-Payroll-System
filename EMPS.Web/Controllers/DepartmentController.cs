using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMPS.Core.Entities;
using EMPS.Core.Interfaces.Services;
using EMPS.Web.Models;

namespace EMPS.Web.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly IMapper _mapper;

        public DepartmentController(IDepartmentService departmentService, IMapper mapper)
        {
            _departmentService = departmentService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            var model = _mapper.Map<IEnumerable<DepartmentViewModel>>(departments);
            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new DepartmentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _departmentService.DepartmentExistsAsync(model.Code))
                {
                    ModelState.AddModelError("Code", "A department with this code already exists.");
                    return View(model);
                }

                var department = _mapper.Map<Department>(model);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                
                await _departmentService.CreateDepartmentAsync(department, userId);
                TempData["SuccessMessage"] = "Department created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<DepartmentViewModel>(department);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _departmentService.DepartmentExistsAsync(model.Code, id))
                {
                    ModelState.AddModelError("Code", "A department with this code already exists.");
                    return View(model);
                }

                var department = await _departmentService.GetDepartmentByIdAsync(id);
                if (department == null)
                {
                    return NotFound();
                }

                _mapper.Map(model, department);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";

                await _departmentService.UpdateDepartmentAsync(department, userId);
                TempData["SuccessMessage"] = "Department updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            await _departmentService.DeleteDepartmentAsync(id, userId);
            TempData["SuccessMessage"] = "Department deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
