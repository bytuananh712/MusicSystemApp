using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Songs
{
    public class UploadFileResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FileUrl { get; set; }
        public long FileSize { get; set; }
        public int Duration { get; set; } // seconds
        public string Format { get; set; }
    }
}
