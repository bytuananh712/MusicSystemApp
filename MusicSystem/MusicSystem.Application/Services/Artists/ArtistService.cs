using MusicSystem.Domain.Entities;
using MusicSystem.Domain.Interfaces;
using MusicSystem.Shared.DTOs.Artists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Artists
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository _artistRepository;
        private readonly IUserRepository _userRepository;

        public ArtistService(
            IArtistRepository artistRepository,
            IUserRepository userRepository)
        {
            _artistRepository = artistRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<ArtistDto>> GetAllArtistsAsync()
        {
            var artists = await _artistRepository.GetAllAsync();
            return artists.Select(MapToDto);
        }

        public async Task<ArtistDto> GetArtistByIdAsync(Guid artistId)
        {
            var artist = await _artistRepository.GetByIdAsync(artistId);
            if (artist == null)
                throw new Exception($"Artist with ID {artistId} not found");

            return MapToDto(artist);
        }

        public async Task<IEnumerable<ArtistDto>> SearchArtistsAsync(string searchTerm)
        {
            var allArtists = await _artistRepository.GetAllAsync();
            return allArtists
                .Where(a => a.ArtistName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Select(MapToDto);
        }

        public async Task<ArtistDto> CreateArtistAsync(CreateArtistDto dto, Guid createdBy)
        {
            // Check if artist name already exists
            var existing = await _artistRepository.GetByNameAsync(dto.ArtistName);
            if (existing != null)
                throw new Exception("Nghệ sĩ này đã tồn tại trong hệ thống");

            var artist = new Artist
            {
                ArtistId = Guid.NewGuid(),
                ArtistName = dto.ArtistName,
                Biography = dto.Biography,
                AvatarUrl = dto.AvatarUrl ?? $"https://ui-avatars.com/api/?name={dto.ArtistName}&background=random",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = createdBy
            };

            await _artistRepository.AddAsync(artist);
            return MapToDto(artist);
        }

        public async Task<ArtistDto> UpdateArtistAsync(Guid artistId, UpdateArtistDto dto)
        {
            var artist = await _artistRepository.GetByIdAsync(artistId);
            if (artist == null)
                throw new Exception($"Artist with ID {artistId} not found");

            // Check duplicate name (nếu đổi tên)
            if (!string.IsNullOrEmpty(dto.ArtistName) && dto.ArtistName != artist.ArtistName)
            {
                var existing = await _artistRepository.GetByNameAsync(dto.ArtistName);
                if (existing != null)
                    throw new Exception("Tên nghệ sĩ này đã tồn tại");
            }

            artist.ArtistName = dto.ArtistName ?? artist.ArtistName;
            artist.Biography = dto.Biography ?? artist.Biography;
            artist.AvatarUrl = dto.AvatarUrl ?? artist.AvatarUrl;

            await _artistRepository.UpdateAsync(artist);
            return MapToDto(artist);
        }

        public async Task<bool> DeleteArtistAsync(Guid artistId)
        {
            // TODO: Check if artist has songs
            // Nếu có bài hát thì không cho xóa, chỉ disable
            await _artistRepository.DeleteAsync(artistId);
            return true;
        }

        public async Task<bool> DisableArtistAsync(Guid artistId)
        {
            return await _artistRepository.DisableAsync(artistId);
        }

        public async Task<bool> EnableArtistAsync(Guid artistId)
        {
            return await _artistRepository.EnableAsync(artistId);
        }

        // Helper
        private ArtistDto MapToDto(Artist artist)
        {
            return new ArtistDto
            {
                ArtistId = artist.ArtistId,
                ArtistName = artist.ArtistName,
                AvatarUrl = artist.AvatarUrl,
                Biography = artist.Biography,
                Status = artist.Status,
                CreatedAt = artist.CreatedAt ,
                UpdatedAt = artist.UpdatedAt ?? DateTime.MinValue
            };
        }
    }
}
