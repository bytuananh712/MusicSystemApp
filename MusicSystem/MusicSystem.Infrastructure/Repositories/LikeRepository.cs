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
    public class LikeRepository : ILikeRepository
    {
        private readonly MusicStreamingDbContext _context;

        public LikeRepository(MusicStreamingDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleLikeAsync(Guid userId, Guid songId)
        {
            var existingLike = await _context.SongLikes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.SongId == songId);

            if (existingLike != null)
            {
                // Unlike
                _context.SongLikes.Remove(existingLike);

                // Decrease like count
                var song = await _context.Songs.FindAsync(songId);
                if (song != null)
                {
                    song.TotalLikes = Math.Max(0, song.TotalLikes - 1);
                }

                await _context.SaveChangesAsync();
                return false; // unliked
            }
            else
            {
                // Like
                var like = new SongLike
                {
                    SongLikeId = Guid.NewGuid(),
                    UserId = userId,
                    SongId = songId,
                    LikedAt = DateTime.UtcNow
                };

                await _context.SongLikes.AddAsync(like);

                // Increase like count
                var song = await _context.Songs.FindAsync(songId);
                if (song != null)
                {
                    song.TotalLikes++;
                }

                await _context.SaveChangesAsync();
                return true; // liked
            }
        }

        public async Task<bool> IsLikedAsync(Guid userId, Guid songId)
        {
            return await _context.SongLikes
                .AnyAsync(l => l.UserId == userId && l.SongId == songId);
        }

        public async Task<int> GetLikeCountAsync(Guid songId)
        {
            return await _context.SongLikes
                .CountAsync(l => l.SongId == songId);
        }

        public async Task<IEnumerable<SongLike>> GetLikedSongsAsync(Guid userId)
        {
            return await _context.SongLikes
                .Where(l => l.UserId == userId)
                .Include(l => l.Song)
                    .ThenInclude(s => s.SongArtists)
                        .ThenInclude(sa => sa.Artist)
                .OrderByDescending(l => l.LikedAt)
                .ToListAsync();
        }
    }
}
