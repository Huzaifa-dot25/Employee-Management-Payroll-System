using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMPS.Web.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        public IActionResult Index()
        {
            // Placeholder for Reports module
            return View();
        }
    }
}
