using MusicSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Domain.Interfaces
{
    public interface IHistoryRepository
    {
        Task TrackPlayAsync(Guid userId, Guid songId);
        Task<IEnumerable<ListeningHistory>> GetUserHistoryAsync(Guid userId, int limit = 50);
    }
}
