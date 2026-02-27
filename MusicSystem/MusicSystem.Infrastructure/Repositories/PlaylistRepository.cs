using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicSystem.Domain.Entities;
using MusicSystem.Domain.Interfaces;
using MusicSystem.Infrastructure.Data;

namespace MusicSystem.Infrastructure.Repositories
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly MusicStreamingDbContext _context;

        public PlaylistRepository(MusicStreamingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Playlist>> GetUserPlaylistsAsync(Guid userId)
        {
            return await _context.Playlists
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Playlist> GetByIdAsync(Guid playlistId)
        {
            return await _context.Playlists
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                    .ThenInclude(s => s.SongArtists)
                    .ThenInclude(sa => sa.Artist)
                .FirstOrDefaultAsync(p => p.PlaylistId == playlistId);
        }

        public async Task<Playlist> CreateAsync(Playlist playlist)
        {
            await _context.Playlists.AddAsync(playlist);
            await _context.SaveChangesAsync();
            return playlist;
        }

        public async Task AddSongAsync(Guid playlistId, Guid songId)
        {
            var exists = await _context.PlaylistSongs
                .AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (exists) return;

            var playlistSong = new PlaylistSong
            {
                PlaylistSongId = Guid.NewGuid(),
                PlaylistId = playlistId,
                SongId = songId,
                AddedAt = DateTime.UtcNow
            };

            await _context.PlaylistSongs.AddAsync(playlistSong);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveSongAsync(Guid playlistId, Guid songId)
        {
            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (playlistSong != null)
            {
                _context.PlaylistSongs.Remove(playlistSong);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid playlistId)
        {
            var playlist = await _context.Playlists.FindAsync(playlistId);
            if (playlist != null)
            {
                _context.Playlists.Remove(playlist);
                await _context.SaveChangesAsync();
            }
        }
    }
}