using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Songs
{
    public class CreateSongDto
    {
        [Required(ErrorMessage = "Tiêu đề bài hát là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được quá 255 ký tự")]
        public string Title { get; set; }

        public string Lyrics { get; set; }

        // File info (sẽ được set sau khi upload)
        public string FileUrl { get; set; }
        public long? FileSize { get; set; }
        public string Format { get; set; }
        public int Duration { get; set; } // seconds

        // Metadata
        public string Genre { get; set; }
        public int? ReleaseYear { get; set; }
        public string CoverImageUrl { get; set; }

        // Artists
        [Required(ErrorMessage = "Phải chọn ít nhất 1 nghệ sĩ")]
        public List<Guid> ArtistIds { get; set; } = new List<Guid>();
    }
}
