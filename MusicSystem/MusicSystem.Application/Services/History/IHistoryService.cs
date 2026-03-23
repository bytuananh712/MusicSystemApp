using MusicSystem.Shared.DTOs.Songs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.History
{
    public interface IHistoryService
    {
        Task TrackPlayAsync(Guid userId, Guid songId);
        Task<IEnumerable<HistoryDto>> GetUserHistoryAsync(Guid userId, int limit = 50);
    }
}
