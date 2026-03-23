using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicSystem.Application.Services.Likes;
using MusicSystem.Application.Services.History;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MusicSystem.Web.Controllers
{
    [Route("Song")]
    public class SongController : Controller
    {
        private readonly ILikeService _likeService;
        private readonly IHistoryService _historyService;

        public SongController(
            ILikeService likeService,
            IHistoryService historyService)
        {
            _likeService = likeService;
            _historyService = historyService;
        }

        [HttpGet("Liked")]
        [Authorize]
        public async Task<IActionResult> LikedSongs()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return RedirectToAction("Login", "Account");

            var likedSongs = await _likeService.GetLikedSongsAsync(userId);
            return View(likedSongs);
        }

        [HttpPost("Like/{songId}")]
        [Authorize]
        public async Task<IActionResult> Like(Guid songId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                var liked = await _likeService.ToggleLikeAsync(userId, songId);

                return Json(new { success = true, liked });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("TrackPlay/{songId}")]
        public async Task<IActionResult> TrackPlay(Guid songId)
        {
            try
            {
                var userId = GetCurrentUserId();

                if (userId == Guid.Empty)
                {
                    return Json(new { success = true, message = "Anonymous play" });
                }

                await _historyService.TrackPlayAsync(userId, songId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("IsLiked/{songId}")]
        public async Task<IActionResult> IsLiked(Guid songId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Json(new { liked = false });
                }

                var liked = await _likeService.IsLikedAsync(userId, songId);

                return Json(new { liked });
            }
            catch
            {
                return Json(new { liked = false });
            }
        }

        // Helper: Get UserId from Claims
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