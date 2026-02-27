using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicSystem.Domain.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MusicSystem.Web.Controllers
{
    [Route("Song")]
    public class SongController : Controller
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IHistoryRepository _historyRepository;

        public SongController(
            ILikeRepository likeRepository,
            IHistoryRepository historyRepository)
        {
            _likeRepository = likeRepository;
            _historyRepository = historyRepository;
        }

        [HttpGet("Liked")]
        [Authorize]
        public async Task<IActionResult> LikedSongs()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return RedirectToAction("Login", "Account");

            var likedSongs = await _likeRepository.GetLikedSongsAsync(userId);
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

                var liked = await _likeRepository.ToggleLikeAsync(userId, songId);

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

                await _historyRepository.TrackPlayAsync(userId, songId);

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

                var liked = await _likeRepository.IsLikedAsync(userId, songId);

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