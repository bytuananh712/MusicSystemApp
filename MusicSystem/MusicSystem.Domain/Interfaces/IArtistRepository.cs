using MusicSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Domain.Interfaces
{
    public interface IArtistRepository
    {
        Task<IEnumerable<Artist>> GetAllAsync();
        Task<Artist> GetByIdAsync(Guid artistId);
        Task<Artist> GetByNameAsync(string artistName);
        Task<Artist> AddAsync(Artist artist);
        Task UpdateAsync(Artist artist);
        Task DeleteAsync(Guid artistId);
        Task<bool> DisableAsync(Guid artistId);
        Task<bool> EnableAsync(Guid artistId);
        Task<int> CountAsync(string status = null);
    }
}
