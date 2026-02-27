using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Playlists
{
    public class SongDto
    {
        public Guid SongId { get; set; }
        public string Title { get; set; }
        public string Artists { get; set; }
        public int Duration { get; set; }
        public string FileUrl { get; set; }
    }
}
