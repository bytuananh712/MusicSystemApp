using MusicSystem.Shared.DTOs.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Playlists
{
    public class PlaylistDto
    {
        public Guid PlaylistId { get; set; }
        public string Title { get; set; }
        public Guid UserId { get; set; }

        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SongCount { get; set; }
        public List<SongDto> Songs { get; set; }
    }
}
