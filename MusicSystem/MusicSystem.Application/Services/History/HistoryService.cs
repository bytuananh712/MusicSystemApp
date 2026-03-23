using MusicSystem.Domain.Entities;
using MusicSystem.Domain.Interfaces;
using MusicSystem.Shared.DTOs.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.History
{
    public class HistoryService : IHistoryService
    {
        private readonly IHistoryRepository _historyRepository;

        public HistoryService(IHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
        }

        public async Task TrackPlayAsync(Guid userId, Guid songId)
        {
            await _historyRepository.TrackPlayAsync(userId, songId);
        }

        public async Task<IEnumerable<HistoryDto>> GetUserHistoryAsync(Guid userId, int limit = 50)
        {
            var history = await _historyRepository.GetUserHistoryAsync(userId, limit);
            return history.Select(MapToDto);
        }

        private HistoryDto MapToDto(ListeningHistory lh)
        {
            return new HistoryDto
            {
                HistoryId = lh.HistoryId,
                SongId = lh.SongId,
                PlayedAt = lh.PlayedAt,
                Title = lh.Song?.Title ?? "Unknown",
                FileUrl = lh.Song?.FileUrl,
                Artists = lh.Song?.SongArtists != null
                    ? string.Join(", ", lh.Song.SongArtists.Select(sa => sa.Artist?.ArtistName ?? "Unknown"))
                    : "Unknown"
            };
        }
    }
}
