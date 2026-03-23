using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicSystem.Application.Services.History;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MusicSystem.Web.Controllers
{
    [Authorize]
    [Route("History")]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        // GET: /History
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            var history = await _historyService.GetUserHistoryAsync(userId, 100);
            return View(history);
        }

        private Guid GetCurrentUserId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }
            }
            return Guid.Empty;
        }

    }
}