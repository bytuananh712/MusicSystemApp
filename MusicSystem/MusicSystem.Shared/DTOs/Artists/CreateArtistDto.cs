using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Artists
{
    public class CreateArtistDto
    {
        [Required(ErrorMessage = "Tên nghệ sĩ là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên nghệ sĩ không được quá 200 ký tự")]
        public string ArtistName { get; set; }

        [StringLength(1000, ErrorMessage = "Tiểu sử không được quá 1000 ký tự")]
        public string Biography { get; set; }

        public string AvatarUrl { get; set; }
    }
}
