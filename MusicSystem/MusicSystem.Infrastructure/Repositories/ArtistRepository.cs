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
    public class ArtistRepository : IArtistRepository
    {
        private readonly MusicStreamingDbContext _context;

        public ArtistRepository(MusicStreamingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Artist>> GetAllAsync()
        {
            return await _context.Artists
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Artist> GetByIdAsync(Guid artistId)
        {
            return await _context.Artists.FindAsync(artistId);
        }

        public async Task<Artist> GetByNameAsync(string artistName)
        {
            return await _context.Artists
                .FirstOrDefaultAsync(a => a.ArtistName == artistName);
        }

        public async Task<Artist> AddAsync(Artist artist)
        {
            await _context.Artists.AddAsync(artist);
            await _context.SaveChangesAsync();
            return artist;
        }

        public async Task UpdateAsync(Artist artist)
        {
            artist.UpdatedAt = DateTime.UtcNow;
            _context.Artists.Update(artist);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid artistId)
        {
            var artist = await _context.Artists.FindAsync(artistId);
            if (artist != null)
            {
                _context.Artists.Remove(artist);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DisableAsync(Guid artistId)
        {
            var artist = await _context.Artists.FindAsync(artistId);
            if (artist == null) return false;

            artist.Status = "Disabled";
            artist.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EnableAsync(Guid artistId)
        {
            var artist = await _context.Artists.FindAsync(artistId);
            if (artist == null) return false;

            artist.Status = "Active";
            artist.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountAsync(string status = null)
        {
            var query = _context.Artists.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            return await query.CountAsync();
        }
    }
}
