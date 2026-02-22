using MusicSystem.Shared.DTOs.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Songs
{
    public interface ISongService
    {
        // Queries
        Task<IEnumerable<SongDto>> GetAllSongsAsync(string status = null, int pageNumber = 1, int pageSize = 20);
        Task<SongDto> GetSongByIdAsync(Guid songId);
        Task<IEnumerable<SongDto>> GetPendingSongsAsync();
        Task<IEnumerable<SongDto>> SearchSongsAsync(string searchTerm);

        // Commands
        Task<SongDto> CreateSongAsync(CreateSongDto dto, Guid createdBy);
        Task<SongDto> UpdateSongAsync(Guid songId, UpdateSongDto dto);
        Task<bool> DeleteSongAsync(Guid songId);
        Task<bool> ApproveSongAsync(Guid songId, Guid managerUserId);
        Task<bool> RejectSongAsync(Guid songId, Guid managerUserId, string reason);
    }
}
