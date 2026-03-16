using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicSystem.Domain.Entities;
using MusicSystem.Domain.Interfaces;
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
        private readonly IPlaylistRepository _playlistRepository;

        public PlaylistController(IPlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            var playlists = await _playlistRepository.GetUserPlaylistsAsync(userId);
            return View(playlists);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var playlist = await _playlistRepository.GetByIdAsync(id);
            if (playlist == null)
            {
                return NotFound();
            }

            return View(playlist);
        }

        
        [HttpPost("Create")]
        public async Task<IActionResult> Create(string title, bool isPublic = true)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var playlist = new Playlist
                {
                    PlaylistId = Guid.NewGuid(),
                    Title = title,          
                    IsPublic = isPublic,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _playlistRepository.CreateAsync(playlist);

                return Json(new { success = true, playlistId = playlist.PlaylistId });
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

                var playlists = await _playlistRepository.GetUserPlaylistsAsync(userId);

                var result = playlists.Select(p => new
                {
                    playlistId = p.PlaylistId,
                    playlistName = p.Title,
                    songCount = p.PlaylistSongs?.Count ?? 0
                });

                return Json(new { success = true, playlists = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost("AddSong")]
        public async Task<IActionResult> AddSong(Guid playlistId, Guid songId)
        {
            try
            {
                await _playlistRepository.AddSongAsync(playlistId, songId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("RemoveSong")]
        public async Task<IActionResult> RemoveSong(Guid playlistId, Guid songId)
        {
            try
            {
                await _playlistRepository.RemoveSongAsync(playlistId, songId);
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
                await _playlistRepository.DeleteAsync(id);
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