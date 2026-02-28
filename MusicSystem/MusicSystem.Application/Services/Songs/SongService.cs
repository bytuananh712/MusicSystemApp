using MusicSystem.Domain.Entities;
using MusicSystem.Domain.Interfaces;
using MusicSystem.Shared.DTOs.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Songs
{
    public class SongService : ISongService
    {
        private readonly ISongRepository _songRepository;
        private readonly IArtistRepository _artistRepository;

        public SongService(
            ISongRepository songRepository,
            IArtistRepository artistRepository)
        {
            _songRepository = songRepository;
            _artistRepository = artistRepository;
        }

        public async Task<IEnumerable<SongDto>> GetAllSongsAsync(string status = null, int pageNumber = 1, int pageSize = 20)
        {
            var songs = await _songRepository.GetAllAsync(status, pageNumber, pageSize);
            return songs.Select(MapToDto);
        }

        public async Task<SongDto> GetSongByIdAsync(Guid songId)
        {
            var song = await _songRepository.GetByIdAsync(songId);
            if (song == null)
                throw new Exception($"Song with ID {songId} not found");

            return MapToDto(song);
        }

        public async Task<IEnumerable<SongDto>> GetPendingSongsAsync()
        {
            var songs = await _songRepository.GetPendingSongsAsync();
            return songs.Select(MapToDto);
        }

        public async Task<IEnumerable<SongDto>> SearchSongsAsync(string searchTerm)
        {
            var allSongs = await _songRepository.GetAllAsync(null, 1, 1000);
            return allSongs
                .Where(s => s.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Select(MapToDto);
        }

        public async Task<SongDto> CreateSongAsync(CreateSongDto dto, Guid createdBy)
        {
            // Validate artists
            foreach (var artistId in dto.ArtistIds)
            {
                var artist = await _artistRepository.GetByIdAsync(artistId);
                if (artist == null)
                    throw new Exception($"Artist with ID {artistId} not found");
            }

            // Check for duplicates (same title and same artists)
            var allSongs = await _songRepository.GetAllAsync(null, 1, 10000);
            var isDuplicate = allSongs.Any(s =>
                s.Title.Equals(dto.Title, StringComparison.OrdinalIgnoreCase) &&
                s.SongArtists != null &&
                s.SongArtists.Count == dto.ArtistIds.Count &&
                s.SongArtists.All(sa => dto.ArtistIds.Contains(sa.ArtistId))
            );

            if (isDuplicate)
            {
                throw new Exception("Bài hát này đã tồn tại trong hệ thống (trùng Tên và Nghệ sĩ)!");
            }

            var song = new Song
            {
                SongId = Guid.NewGuid(),
                Title = dto.Title,
                Duration = dto.Duration,
                Lyrics = dto.Lyrics,
                FileUrl = dto.FileUrl,
                FileSize = dto.FileSize,
                Format = dto.Format,
                Genre = dto.Genre,
                ReleaseYear = dto.ReleaseYear,
                CoverImageUrl = dto.CoverImageUrl,
                Status = "Pending",
                TotalPlays = 0,
                TotalLikes = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            // Add song-artist relationships
            song.SongArtists = dto.ArtistIds.Select(artistId => new SongArtist
            {
                SongId = song.SongId,
                ArtistId = artistId
            }).ToList();

            await _songRepository.AddAsync(song);

            // Fetch the inserted song with Artist includes so the DTO mapping has artist names
            var savedSong = await _songRepository.GetByIdAsync(song.SongId);

            return MapToDto(savedSong ?? song);
        }

        public async Task<SongDto> UpdateSongAsync(Guid songId, UpdateSongDto dto)
        {
            var song = await _songRepository.GetByIdAsync(songId);
            if (song == null)
                throw new Exception($"Song with ID {songId} not found");

            song.Title = dto.Title ?? song.Title;
            song.Duration = dto.Duration > 0 ? dto.Duration : song.Duration;
            song.Lyrics = dto.Lyrics ?? song.Lyrics;
            song.Genre = dto.Genre ?? song.Genre;
            song.ReleaseYear = dto.ReleaseYear ?? song.ReleaseYear;
            song.CoverImageUrl = dto.CoverImageUrl ?? song.CoverImageUrl;

            await _songRepository.UpdateAsync(song);

            return MapToDto(song);
        }

        public async Task<bool> DeleteSongAsync(Guid songId)
        {
            await _songRepository.DeleteAsync(songId);
            return true;
        }

        public async Task<bool> ApproveSongAsync(Guid songId, Guid managerUserId)
        {
            return await _songRepository.ApproveSongAsync(songId, managerUserId);
        }

        public async Task<bool> RejectSongAsync(Guid songId, Guid managerUserId, string reason)
        {
            var song = await _songRepository.GetByIdAsync(songId);
            if (song == null) return false;

            song.Status = "Disabled";
            await _songRepository.UpdateAsync(song);



            return true;
        }

        // Helper
        private SongDto MapToDto(Song song)
        {
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
