using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Artists
{
    public class UpdateArtistDto
    {
        public string ArtistName { get; set; }
        public string Biography { get; set; }
        public string AvatarUrl { get; set; }
    }
}
