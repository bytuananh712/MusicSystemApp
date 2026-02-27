using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Songs
{
    public class HistoryDto
    {
        public Guid HistoryId { get; set; }
        public Guid SongId { get; set; }
        public string Title { get; set; }
        public string Artists { get; set; }
        public DateTime PlayedAt { get; set; }
        public string FileUrl { get; set; }
    }
}
