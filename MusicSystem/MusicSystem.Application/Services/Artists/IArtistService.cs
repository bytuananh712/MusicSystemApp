using MusicSystem.Shared.DTOs.Artists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Artists
{
    public interface IArtistService
    {
        // Queries
        Task<IEnumerable<ArtistDto>> GetAllArtistsAsync();
        Task<ArtistDto> GetArtistByIdAsync(Guid artistId);
        Task<IEnumerable<ArtistDto>> SearchArtistsAsync(string searchTerm);

        // Commands
        Task<ArtistDto> CreateArtistAsync(CreateArtistDto dto);
        Task<ArtistDto> UpdateArtistAsync(Guid artistId, UpdateArtistDto dto);
        Task<bool> DeleteArtistAsync(Guid artistId);
        Task<bool> DisableArtistAsync(Guid artistId);
        Task<bool> EnableArtistAsync(Guid artistId);
    }
}
