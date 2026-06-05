using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMPS.Core.Interfaces.Services;

namespace EMPS.Web.Controllers
{
    /// <summary>
    /// Lightweight lookup endpoint accessible to ALL authenticated users.
    /// Used for dynamic dropdowns (e.g. designations by department) in forms.
    /// </summary>
    [Authorize]
    public class LookupController : Controller
    {
        private readonly IDesignationService _designationService;

        public LookupController(IDesignationService designationService)
        {
            _designationService = designationService;
        }

        /// <summary>
        /// Returns designations for a given department as [{id, name}] JSON.
        /// Called via AJAX from the Employee create/edit forms.
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> DesignationsByDepartment(int departmentId)
        {
            var designations = await _designationService.GetDesignationsByDepartmentAsync(departmentId);
            var result = designations
                .OrderBy(d => d.Name)
                .Select(d => new { id = d.Id, name = d.Name });
            return Json(result);
        }
    }
}
