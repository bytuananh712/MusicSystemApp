using MusicSystem.Shared.DTOs.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Files
{
    public interface IFileUploadService
    {
        Task<UploadFileResponse> UploadSongFileAsync(UploadFileRequest request);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<int> GetAudioDurationAsync(string filePath);
    }
}
