using MusicSystem.Shared.DTOs.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Application.Services.Files
{
    public class FileUploadService : IFileUploadService
    {
        private readonly string _uploadFolder;

        // Constructor nhận path trực tiếp (sẽ inject từ Program.cs)
        public FileUploadService(string uploadFolderPath)
        {
            _uploadFolder = uploadFolderPath;

            // Ensure folder exists
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        public async Task<UploadFileResponse> UploadSongFileAsync(UploadFileRequest request)
        {
            try
            {
                // Validate file
                if (request.FileData == null || request.FileData.Length == 0)
                {
                    return new UploadFileResponse
                    {
                        Success = false,
                        Message = "File không hợp lệ"
                    };
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(request.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_uploadFolder, uniqueFileName);

                // Save file
                await File.WriteAllBytesAsync(filePath, request.FileData);

                // Get file info
                var fileInfo = new FileInfo(filePath);
                var fileSize = fileInfo.Length;

                // Get duration (simplified)
                var duration = await GetAudioDurationAsync(filePath);

                // Get format
                var format = fileExtension.TrimStart('.').ToUpper();

                // Generate URL
                var fileUrl = $"/uploads/songs/{uniqueFileName}";

                return new UploadFileResponse
                {
                    Success = true,
                    Message = "Upload thành công",
                    FileUrl = fileUrl,
                    FileSize = fileSize,
                    Duration = duration,
                    Format = format
                };
            }
            catch (Exception ex)
            {
                return new UploadFileResponse
                {
                    Success = false,
                    Message = $"Lỗi upload: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return false;

                // Extract filename from URL
                var fileName = Path.GetFileName(fileUrl);
                var filePath = Path.Combine(_uploadFolder, fileName);

                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetAudioDurationAsync(string filePath)
        {
            try
            {
                await Task.CompletedTask;

                // Estimate based on file size (rough approximation)
                var fileInfo = new FileInfo(filePath);
                var fileSizeInMB = fileInfo.Length / 1024.0 / 1024.0;

                // Assume 1MB ≈ 1 minute for MP3 @ 128kbps
                var estimatedDuration = (int)(fileSizeInMB * 60);

                // Return between 30s and 600s (10 minutes)
                return Math.Max(30, Math.Min(estimatedDuration, 600));
            }
            catch
            {
                return 180; // Default 3 minutes
            }
        }
    }
}
