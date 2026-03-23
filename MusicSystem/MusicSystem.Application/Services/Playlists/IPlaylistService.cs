using MusicSystem.Shared.DTOs.Playlists;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Playlists
{
    public interface IPlaylistService
    {
        Task<IEnumerable<PlaylistDto>> GetUserPlaylistsAsync(Guid userId);
        Task<PlaylistDto> GetByIdAsync(Guid playlistId);
        Task<PlaylistDto> CreateAsync(string title, bool isPublic, Guid userId);
        Task AddSongAsync(Guid playlistId, Guid songId);
        Task RemoveSongAsync(Guid playlistId, Guid songId);
        Task DeleteAsync(Guid playlistId);
    }
}
