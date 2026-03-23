using MusicSystem.Shared.DTOs.Songs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Likes
{
    public interface ILikeService
    {
        Task<bool> ToggleLikeAsync(Guid userId, Guid songId);
        Task<bool> IsLikedAsync(Guid userId, Guid songId);
        Task<int> GetLikeCountAsync(Guid songId);
        Task<IEnumerable<SongDto>> GetLikedSongsAsync(Guid userId);
    }
}
