using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EMPS.Core.Entities;
using EMPS.Core.Interfaces.Services;
using EMPS.Web.Models;

namespace EMPS.Web.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class DesignationController : Controller
    {
        private readonly IDesignationService _designationService;
        private readonly IDepartmentService _departmentService;
        private readonly IMapper _mapper;

        public DesignationController(
            IDesignationService designationService,
            IDepartmentService departmentService,
            IMapper mapper)
        {
            _designationService = designationService;
            _departmentService = departmentService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var designations = await _designationService.GetAllDesignationsAsync();
            var model = _mapper.Map<IEnumerable<DesignationViewModel>>(designations);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDepartmentsSelectListAsync();
            return View(new DesignationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DesignationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var designation = _mapper.Map<Designation>(model);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";

                await _designationService.CreateDesignationAsync(designation, userId);
                TempData["SuccessMessage"] = "Designation created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDepartmentsSelectListAsync(model.DepartmentId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var designation = await _designationService.GetDesignationByIdAsync(id);
            if (designation == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<DesignationViewModel>(designation);
            await PopulateDepartmentsSelectListAsync(model.DepartmentId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DesignationViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var designation = await _designationService.GetDesignationByIdAsync(id);
                if (designation == null)
                {
                    return NotFound();
                }

                _mapper.Map(model, designation);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";

                await _designationService.UpdateDesignationAsync(designation, userId);
                TempData["SuccessMessage"] = "Designation updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDepartmentsSelectListAsync(model.DepartmentId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            await _designationService.DeleteDesignationAsync(id, userId);
            TempData["SuccessMessage"] = "Designation deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDepartmentsSelectListAsync(object? selectedValue = null)
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            ViewBag.DepartmentsList = new SelectList(departments, "Id", "Name", selectedValue);
        }
    }
}
