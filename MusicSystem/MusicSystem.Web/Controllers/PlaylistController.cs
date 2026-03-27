using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicSystem.Domain.Entities;
using MusicSystem.Application.Services.Playlists;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MusicSystem.Web.Controllers
{
    [Authorize]
    [Route("Playlist")]
    public class PlaylistController : Controller
    {
        private readonly IPlaylistService _playlistService;

        public PlaylistController(IPlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            var playlists = await _playlistService.GetUserPlaylistsAsync(userId);
            return View(playlists);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var playlist = await _playlistService.GetByIdAsync(id);
            if (playlist == null)
            {
                return NotFound();
            }

            return View(playlist);
        }


        [HttpPost("Create")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(string title, bool isPublic = true)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var createdPlaylist = await _playlistService.CreateAsync(title, isPublic, userId);

                return Json(new { success = true, playlistId = createdPlaylist.PlaylistId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetUserPlaylists")]
        public async Task<IActionResult> GetUserPlaylists()
        {
            try
            {
                var userId = GetCurrentUserId();

                if (userId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Chưa đăng nhập" });
                }

                var playlists = await _playlistService.GetUserPlaylistsAsync(userId);

                var result = playlists.Select(p => new
                {
                    playlistId = p.PlaylistId,
                    playlistName = p.Title,
                    songCount = p.SongCount
                });

                return Json(new { success = true, playlists = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost("AddSong")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddSong(Guid playlistId, Guid songId)
        {
            try
            {
                if (playlistId == Guid.Empty || songId == Guid.Empty)
                    return Json(new { success = false, message = "ID không hợp lệ" });

                var userId = GetCurrentUserId();
                await _playlistService.AddSongAsync(playlistId, songId, userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("RemoveSong")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RemoveSong(Guid playlistId, Guid songId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _playlistService.RemoveSongAsync(playlistId, songId, userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _playlistService.DeleteAsync(id, userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper: Get current user ID
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