using Microsoft.EntityFrameworkCore;
using MusicSystem.Domain.Entities;
using MusicSystem.Domain.Interfaces;
using MusicSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Infrastructure.Repositories
{
    public class SongRepository : ISongRepository
    {
        private readonly MusicStreamingDbContext _context;

        public SongRepository(MusicStreamingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Song>> GetAllAsync(string status, int pageNumber, int pageSize)
        {
            var query = _context.Songs
                .Include(s => s.SongArtists)
                    .ThenInclude(sa => sa.Artist)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
                
                // Ẩn bài hát nếu có bất kỳ nghệ sĩ nào bị khoá (Status == "Disabled")
                if (status == "Active")
                {
                    query = query.Where(s => !s.SongArtists.Any(sa => sa.Artist.Status == "Disabled"));
                }
            }

            return await query
                .OrderBy(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Song> GetByIdAsync(Guid songId)
        {
            return await _context.Songs
                .Include(s => s.SongArtists)
                    .ThenInclude(sa => sa.Artist)
                .FirstOrDefaultAsync(s => s.SongId == songId);
        }

        public async Task<IEnumerable<Song>> GetPendingSongsAsync()
        {
            return await _context.Songs
                .Include(s => s.SongArtists)
                    .ThenInclude(sa => sa.Artist)
                .Where(s => s.Status == "Pending")
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Song> AddAsync(Song song)
        {
            await _context.Songs.AddAsync(song);
            await _context.SaveChangesAsync();
            return song;
        }

        public async Task UpdateAsync(Song song)
        {
            _context.Songs.Update(song);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid songId)
        {
            var song = await _context.Songs.FindAsync(songId);
            if (song != null)
            {
                _context.Songs.Remove(song);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ApproveSongAsync(Guid songId, Guid managerUserId)
        {
            var song = await _context.Songs.FindAsync(songId);
            if (song == null || song.Status != "Pending")
                return false;

            song.Status = "Active";
            song.ApprovedBy = managerUserId;
            song.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountAsync(string status = null)
        {
            var query = _context.Songs.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }

            return await query.CountAsync();
        }
    }
}
