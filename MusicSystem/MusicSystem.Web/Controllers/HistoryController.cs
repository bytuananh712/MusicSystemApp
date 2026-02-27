using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicSystem.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MusicSystem.Web.Controllers
{
    [Authorize]
    [Route("History")]
    public class HistoryController : Controller
    {
        private readonly IHistoryRepository _historyRepository;

        public HistoryController(IHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
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

            var history = await _historyRepository.GetUserHistoryAsync(userId, 100);
            return View(history);
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            return Guid.Empty;
        }
    }
}