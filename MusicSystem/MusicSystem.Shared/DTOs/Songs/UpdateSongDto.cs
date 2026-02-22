using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Songs
{
    public class UpdateSongDto
    {
        public string Title { get; set; }
        public int Duration { get; set; }
        public string Lyrics { get; set; }
        public string Genre { get; set; }
        public int? ReleaseYear { get; set; }
        public string CoverImageUrl { get; set; }
        public List<Guid> ArtistIds { get; set; }
    }
}
