using MusicSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Domain.Interfaces
{
    public interface IPlaylistRepository
    {
        Task<IEnumerable<Playlist>> GetUserPlaylistsAsync(Guid userId);
        Task<Playlist> GetByIdAsync(Guid playlistId);
        Task<Playlist> CreateAsync(Playlist playlist);
        Task AddSongAsync(Guid playlistId, Guid songId);
        Task RemoveSongAsync(Guid playlistId, Guid songId);
        Task DeleteAsync(Guid playlistId);
    }
}
