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
    public class HistoryRepository : IHistoryRepository
    {
        private readonly MusicStreamingDbContext _context;

        public HistoryRepository(MusicStreamingDbContext context)
        {
            _context = context;
        }

        public async Task TrackPlayAsync(Guid userId, Guid songId)
        {
            // Debounce: Chặn spam lượt nghe bằng cách kiểm tra lịch sử 5 phút gần nhất
            var recentPlay = await _context.ListeningHistories
                .Where(h => h.UserId == userId && h.SongId == songId && h.PlayedAt >= DateTime.UtcNow.AddMinutes(-5))
                .FirstOrDefaultAsync();

            if (recentPlay != null)
            {
                return; // Đã nghe gần đây, bỏ qua chống spam
            }

            var history = new ListeningHistory
            {
                HistoryId = Guid.NewGuid(),
                UserId = userId,
                SongId = songId,
                PlayedAt = DateTime.UtcNow
            };

            await _context.ListeningHistories.AddAsync(history);

            // Increase play count
            var song = await _context.Songs.FindAsync(songId);
            if (song != null)
            {
                song.TotalPlays++;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ListeningHistory>> GetUserHistoryAsync(Guid userId, int limit = 50)
        {
            return await _context.ListeningHistories
                .Include(h => h.Song)
                    .ThenInclude(s => s.SongArtists)
                    .ThenInclude(sa => sa.Artist)
                .Where(h => h.UserId == userId)
                .OrderBy(h => h.PlayedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}