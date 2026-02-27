using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Songs
{
    public class LikeDto
    {
        public Guid UserSongLikeId { get; set; }
        public Guid UserId { get; set; }
        public Guid SongId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
