using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMPS.Web.Services;

namespace EMPS.Web.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class ChatController : Controller
    {
        private readonly IChatAssistantService _chatAssistantService;

        public ChatController(IChatAssistantService chatAssistantService)
        {
            _chatAssistantService = chatAssistantService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "HR Chat Assistant";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Message cannot be empty." });
            }

            string responseHtml = await _chatAssistantService.ProcessMessageAsync(request.Message);
            return Ok(new { response = responseHtml });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
