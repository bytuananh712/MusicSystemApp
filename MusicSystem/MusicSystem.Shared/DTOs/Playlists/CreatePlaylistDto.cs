using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Playlists
{
    public class CreatePlaylistDto
    {
        [Required(ErrorMessage = "Tiêu đề playlist là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được quá 255 ký tự")]
      
        public string Title { get; set; }
        public bool IsPublic { get; set; } = true; // Mặc định công khai
    }
}
