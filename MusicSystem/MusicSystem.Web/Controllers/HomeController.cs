using Microsoft.AspNetCore.Mvc;
using MusicSystem.Application.Services.Songs;
using MusicSystem.Shared.DTOs.Songs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISongService _songService;

        public HomeController(ISongService songService)
        {
            _songService = songService;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            var songs = await _songService.GetAllSongsAsync("Active", 1, 50);
            return View(songs);
        }

        // GET: /Home/Search?q=...
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return RedirectToAction("Index");
            }

            var songs = await _songService.SearchSongsAsync(q);
            ViewBag.SearchQuery = q;
            return View("Index", songs);
        }

        // API: Get song by ID (for player)
        [HttpGet]
        public async Task<IActionResult> GetSong(Guid id)
        {
            try
            {
                var song = await _songService.GetSongByIdAsync(id);
                return Json(new { success = true, song });
            }
            catch
            {
                return Json(new { success = false, message = "Song not found" });
            }
        }
    }
}