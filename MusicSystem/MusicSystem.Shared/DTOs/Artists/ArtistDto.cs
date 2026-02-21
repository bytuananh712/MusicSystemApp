using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Artists
{
    public class ArtistDto
    {
        public Guid ArtistId { get; set; }
        public string ArtistName { get; set; }
        public string AvatarUrl { get; set; }
        public string Biography { get; set; }
        public string Status { get; set; } // Active/Disabled
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
}
