using MusicSystem.Domain.Entities;
using MusicSystem.Domain.Interfaces;
using MusicSystem.Shared.DTOs.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Likes
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;

        public LikeService(ILikeRepository likeRepository)
        {
            _likeRepository = likeRepository;
        }

        public async Task<bool> ToggleLikeAsync(Guid userId, Guid songId)
        {
            return await _likeRepository.ToggleLikeAsync(userId, songId);
        }

        public async Task<bool> IsLikedAsync(Guid userId, Guid songId)
        {
            return await _likeRepository.IsLikedAsync(userId, songId);
        }

        public async Task<int> GetLikeCountAsync(Guid songId)
        {
            return await _likeRepository.GetLikeCountAsync(songId);
        }

        public async Task<IEnumerable<SongDto>> GetLikedSongsAsync(Guid userId)
        {
            var likedSongs = await _likeRepository.GetLikedSongsAsync(userId);
            return likedSongs.Select(ls => MapToSongDto(ls.Song));
        }

        private SongDto MapToSongDto(Song song)
        {
            if (song == null) return null;
            return new SongDto
            {
                SongId = song.SongId,
                Title = song.Title,
                Duration = song.Duration,
                FileUrl = song.FileUrl,
                FileSize = song.FileSize,
                Format = song.Format,
                Genre = song.Genre,
                ReleaseYear = song.ReleaseYear,
                CoverImageUrl = song.CoverImageUrl,
                Lyrics = song.Lyrics,
                TotalPlays = song.TotalPlays,
                TotalLikes = song.TotalLikes,
                Status = song.Status,
                Artists = song.SongArtists != null
                    ? string.Join(", ", song.SongArtists.Select(sa => sa.Artist?.ArtistName ?? "Unknown"))
                    : "Unknown",
                CreatedAt = song.CreatedAt,
                ApprovedAt = song.ApprovedAt
            };
        }
    }
}
