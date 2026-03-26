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
        private readonly IWebHostEnvironment _env;

        public HomeController(ISongService songService, IWebHostEnvironment env)
        {
            _songService = songService;
            _env = env;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            var songs = await _songService.GetAllSongsAsync("Active", 1, 50);
            var validSongs = FilterExistingFiles(songs);
            return View(validSongs);
        }

        // GET: /Home/Search?q=...
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return RedirectToAction("Index");
            }

            var songs = await _songService.SearchSongsAsync(q);
            var validSongs = FilterExistingFiles(songs);
            ViewBag.SearchQuery = q;
            return View("Index", validSongs);
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

        // Helper: Only return songs whose file actually exists on disk
        private IEnumerable<SongDto> FilterExistingFiles(IEnumerable<SongDto> songs)
        {
            return songs.Where(song =>
            {
                if (string.IsNullOrEmpty(song.FileUrl))
                    return false;

                // FileUrl format: /uploads/songs/{guid}.mp3
                // Map to physical path: {WebRootPath}/uploads/songs/{guid}.mp3
                var relativePath = song.FileUrl.TrimStart('/').Replace('/', System.IO.Path.DirectorySeparatorChar);
                var physicalPath = System.IO.Path.Combine(_env.WebRootPath, relativePath);
                return System.IO.File.Exists(physicalPath);
            });
        }
    }
}