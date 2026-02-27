using MusicSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Domain.Interfaces
{
    public interface ILikeRepository
    {
        Task<bool> ToggleLikeAsync(Guid userId, Guid songId);
        Task<bool> IsLikedAsync(Guid userId, Guid songId);
        Task<int> GetLikeCountAsync(Guid songId);
        Task<IEnumerable<SongLike>> GetLikedSongsAsync(Guid userId);
    }
}
