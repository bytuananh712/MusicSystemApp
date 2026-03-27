using MusicSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Domain.Interfaces
{
    public interface ISongRepository
    {
        Task<IEnumerable<Song>> GetAllAsync(string status, int pageNumber, int pageSize);
        Task<Song> GetByIdAsync(Guid songId);
        Task<IEnumerable<Song>> GetPendingSongsAsync();
        Task<Song> AddAsync(Song song);
        Task UpdateAsync(Song song);
        Task DeleteAsync(Guid songId);
        Task<bool> ApproveSongAsync(Guid songId, Guid managerUserId);
        Task<int> CountAsync(string status = null);
        Task<bool> ExistsAsync(string title, List<Guid> artistIds);
        Task<bool> ExistsByFileAsync(long? fileSize, int duration);
        Task<int> CountSongsByArtistAsync(Guid artistId);
    }
}
