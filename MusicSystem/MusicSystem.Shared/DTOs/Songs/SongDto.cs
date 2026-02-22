using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Songs
{
    public class SongDto
    {
        public Guid SongId { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public string DurationFormatted => TimeSpan.FromSeconds(Duration).ToString(@"mm\:ss");

        // File info
        public string FileUrl { get; set; }
        public long? FileSize { get; set; }
        public string FileSizeFormatted => FileSize.HasValue ? $"{FileSize.Value / 1024.0 / 1024.0:F2} MB" : "N/A";
        public string Format { get; set; }

        // Metadata
        public string Genre { get; set; }
        public int? ReleaseYear { get; set; }
        public string CoverImageUrl { get; set; }
        public string Lyrics { get; set; }

        // Stats
        public long TotalPlays { get; set; }
        public int TotalLikes { get; set; }

        // Status
        public string Status { get; set; }

        // Artists
        public string Artists { get; set; } // "Sơn Tùng M-TP, Đen Vâu"
        public List<Guid> ArtistIds { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
