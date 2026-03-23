using MusicSystem.Domain.Entities;
using MusicSystem.Domain.Interfaces;
using MusicSystem.Shared.DTOs.Playlists;
using MusicSystem.Shared.DTOs.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Playlists
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;

        public PlaylistService(IPlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        public async Task<IEnumerable<PlaylistDto>> GetUserPlaylistsAsync(Guid userId)
        {
            var playlists = await _playlistRepository.GetUserPlaylistsAsync(userId);
            return playlists.Select(MapToDto);
        }

        public async Task<PlaylistDto> GetByIdAsync(Guid playlistId)
        {
            var playlist = await _playlistRepository.GetByIdAsync(playlistId);
            if (playlist == null) return null;
            return MapToDto(playlist);
        }

        public async Task<PlaylistDto> CreateAsync(string title, bool isPublic, Guid userId)
        {
            var playlist = new Playlist
            {
                PlaylistId = Guid.NewGuid(),
                Title = title,
                IsPublic = isPublic,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _playlistRepository.CreateAsync(playlist);
            return MapToDto(playlist);
        }

        public async Task AddSongAsync(Guid playlistId, Guid songId)
        {
            await _playlistRepository.AddSongAsync(playlistId, songId);
        }

        public async Task RemoveSongAsync(Guid playlistId, Guid songId)
        {
            await _playlistRepository.RemoveSongAsync(playlistId, songId);
        }

        public async Task DeleteAsync(Guid playlistId)
        {
            await _playlistRepository.DeleteAsync(playlistId);
        }

        private PlaylistDto MapToDto(Playlist playlist)
        {
            return new PlaylistDto
            {
                PlaylistId = playlist.PlaylistId,
                Title = playlist.Title,
                UserId = playlist.UserId,
                IsPublic = playlist.IsPublic,
                CreatedAt = playlist.CreatedAt,
                SongCount = playlist.PlaylistSongs?.Count ?? 0,
                Songs = playlist.PlaylistSongs?.Select(ps => ps.Song != null ? new MusicSystem.Shared.DTOs.Playlists.SongDto
                {
                    SongId = ps.Song.SongId,
                    Title = ps.Song.Title,
                    Duration = ps.Song.Duration,
                    FileUrl = ps.Song.FileUrl,
                    Artists = ps.Song.SongArtists != null
                        ? string.Join(", ", ps.Song.SongArtists.Select(sa => sa.Artist?.ArtistName ?? "Unknown"))
                        : "Unknown"
                } : null).Where(s => s != null).ToList()
            };
        }
    }
}
