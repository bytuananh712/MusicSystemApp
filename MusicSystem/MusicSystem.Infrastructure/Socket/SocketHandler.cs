using Microsoft.Extensions.Logging;
using MusicSystem.Application.Services.Artists;
using MusicSystem.Application.Services.Auth;
using MusicSystem.Application.Services.Files;
using MusicSystem.Application.Services.Songs;
using MusicSystem.Application.Services.Users;
using MusicSystem.Domain.Interfaces;
using MusicSystem.Shared.Constants;
using MusicSystem.Shared.DTOs.Artists;
using MusicSystem.Shared.DTOs.Auth;
using MusicSystem.Shared.DTOs.Songs;
using MusicSystem.Shared.DTOs.Users;
using MusicSystem.Shared.SocketContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MusicSystem.Infrastructure.Socket
{
    public class SocketHandler
    {
        private readonly ILogger<SocketHandler> _logger;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IArtistService _artistService; // quản lí nghệ sĩ
        private readonly ISongService _songService;    
        private readonly IFileUploadService _fileUploadService;
        private readonly IUserRepository _userRepository;


        public SocketHandler(
            ILogger<SocketHandler> logger,
            IAuthService authService,
             IUserService userService,
              IArtistService artistService, ISongService songService, IFileUploadService fileUploadService, IUserRepository userRepository)
        {
            _logger = logger;
            _authService = authService;
            _userService = userService;
            _artistService = artistService;
            _songService = songService;
            _fileUploadService = fileUploadService;
            _userRepository = userRepository;
        }

        public async Task HandleAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                try
                {
                    // Đọc request (JSON trên 1 dòng)
                    var requestJson = await reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(requestJson))
                    {
                        _logger.LogWarning("⚠️ Empty request received");
                        break;
                    }

                    _logger.LogInformation($"📨 Received: {requestJson}");

                    // Parse request
                    var request = JsonSerializer.Deserialize<SocketRequest>(requestJson);

                    // Process request
                    var response = await ProcessRequestAsync(request);

                    // Send response
                    var responseJson = JsonSerializer.Serialize(response);
                    await writer.WriteLineAsync(responseJson);

                    _logger.LogInformation($"📤 Sent: {responseJson}");
                }
                catch (IOException ioEx)
                {
                    _logger.LogWarning(ioEx, "⚠️ Client disconnected unexpectedly");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing request");

                    // Gửi error response
                    var errorResponse = new SocketResponse
                    {
                        Status = SocketStatus.Error,
                        Message = $"Server error: {ex.Message}",
                        Timestamp = DateTime.UtcNow
                    };

                    var errorJson = JsonSerializer.Serialize(errorResponse);
                    await writer.WriteLineAsync(errorJson);
                }
            }
        }



        private async Task<Guid> GetCurrentUserIdAsync()
        {
            try
            {
                // Lấy user có role Manager hoặc Admin (bất kỳ)
                var users = await _userRepository.GetAllWithRolesAsync();

                var user = users.FirstOrDefault(u =>
                    u.UserRoles.Any(ur =>
                        ur.Role.RoleName == "Manager" ||
                        ur.Role.RoleName == "Admin"));

                if (user != null)
                {
                    _logger.LogInformation($"Using userId: {user.UserId} ({user.Username})");
                    return user.UserId;
                }

                throw new Exception("No valid Manager or Admin user found in database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user ID");
                throw;
            }
        }

        private async Task<SocketResponse> ProcessRequestAsync(SocketRequest request)
        {
            try
            {
                _logger.LogInformation($"🔄 Processing command: {request.Command}");

                switch (request.Command)
                {
                    case SocketCommands.Login:
                        return await HandleLoginAsync(request);

                    case SocketCommands.ValidateToken:
                        return HandleValidateToken(request);

                    case SocketCommands.Logout:
                        return HandleLogout(request);

                    // Admin
                    case SocketCommands.GetAllUsers:
                        return await HandleGetAllUsersAsync(request);

                    case SocketCommands.CreateUser:
                        return await HandleCreateUserAsync(request);

                    case SocketCommands.UpdateUser:
                        return await HandleUpdateUserAsync(request);

                    case SocketCommands.DisableUser:
                        return await HandleDisableUserAsync(request);

                    case SocketCommands.EnableUser:
                        return await HandleEnableUserAsync(request);

                    case SocketCommands.AssignRole:
                        return await HandleAssignRoleAsync(request);

                    case SocketCommands.ResetPassword:
                        return await HandleResetPasswordAsync(request);


                    // ===== ARTIST MANAGEMENT =====
                    case SocketCommands.GetAllArtists:
                        return await HandleGetAllArtistsAsync(request);

                    case SocketCommands.CreateArtist:
                        return await HandleCreateArtistAsync(request);

                    case SocketCommands.UpdateArtist:
                        return await HandleUpdateArtistAsync(request);

                    case SocketCommands.DeleteArtist:
                        return await HandleDeleteArtistAsync(request);

                    case SocketCommands.DisableArtist:
                        return await HandleDisableArtistAsync(request);

                    case SocketCommands.EnableArtist:
                        return await HandleEnableArtistAsync(request);

                    // ===== SONG MANAGEMENT =====

                    case SocketCommands.GetAllSongs:
                        return await HandleGetAllSongsAsync(request);

                    case SocketCommands.GetPendingSongs:
                        return await HandleGetPendingSongsAsync(request);

                    case SocketCommands.UploadSongFile:
                        return await HandleUploadSongFileAsync(request);

                    case SocketCommands.CreateSong:
                        return await HandleCreateSongAsync(request);

                    case SocketCommands.UpdateSong:
                        return await HandleUpdateSongAsync(request);

                    case SocketCommands.DeleteSong:
                        return await HandleDeleteSongAsync(request);

                    case SocketCommands.ApproveSong:
                        return await HandleApproveSongAsync(request);

                    case SocketCommands.RejectSong:
                        return await HandleRejectSongAsync(request);





                    // Thêm các command khác sau...

                    default:
                        return new SocketResponse
                        {
                            RequestId = request.RequestId,
                            Status = SocketStatus.InvalidRequest,
                            Message = $"Unknown command: {request.Command}",
                            Timestamp = DateTime.UtcNow
                        };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $" Error processing command: {request.Command}");
                return new SocketResponse
                {
                    RequestId = request.RequestId,
                    Status = SocketStatus.Error,
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        private async Task<SocketResponse> HandleLoginAsync(SocketRequest request)
        {
            try
            {
                // Parse login request từ JSON
                var loginRequest = JsonSerializer.Deserialize<LoginRequestDto>(request.Data);

                // Gọi AuthService
                var loginResult = await _authService.LoginAsync(loginRequest);

                if (!loginResult.Success)
                {
                    return new SocketResponse
                    {
                        RequestId = request.RequestId,
                        Status = SocketStatus.Unauthorized,
                        Message = loginResult.Message,
                        Timestamp = DateTime.UtcNow
                    };
                }

                // Kiểm tra role (chỉ Admin/Manager mới login được qua WPF)
                if (!loginResult.User.Roles.Contains("Admin") &&
                    !loginResult.User.Roles.Contains("Manager"))
                {
                    return new SocketResponse
                    {
                        RequestId = request.RequestId,
                        Status = SocketStatus.Unauthorized,
                        Message = "Chỉ Admin và Manager mới có thể đăng nhập vào ứng dụng quản trị",
                        Timestamp = DateTime.UtcNow
                    };
                }

                // Success - trả về user data + token
                var responseData = JsonSerializer.Serialize(loginResult);

                return new SocketResponse
                {
                    RequestId = request.RequestId,
                    Status = SocketStatus.Success,
                    Message = "Đăng nhập thành công",
                    Data = responseData,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Login error");
                return new SocketResponse
                {
                    RequestId = request.RequestId,
                    Status = SocketStatus.Error,
                    Message = $"Lỗi đăng nhập: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        // Implement handlers
        private async Task<SocketResponse> HandleGetAllUsersAsync(SocketRequest request)
        {
            var users = await _userService.GetAllUsersAsync();
            return SuccessResponse(request.RequestId, users);
        }

        private async Task<SocketResponse> HandleCreateUserAsync(SocketRequest request)
        {
            var createDto = JsonSerializer.Deserialize<CreateUserDto>(request.Data);
            var user = await _userService.CreateUserAsync(createDto);
            return SuccessResponse(request.RequestId, user);
        }

        private async Task<SocketResponse> HandleUpdateUserAsync(SocketRequest request)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(request.Data);
            var userId = Guid.Parse(data["userId"].ToString());
            var updateDto = JsonSerializer.Deserialize<UpdateUserDto>(data["data"].ToString());

            var user = await _userService.UpdateUserAsync(userId, updateDto);
            return SuccessResponse(request.RequestId, user);
        }

        private async Task<SocketResponse> HandleDisableUserAsync(SocketRequest request)
        {
            var userId = Guid.Parse(request.Data);
            var result = await _userService.DisableUserAsync(userId);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleEnableUserAsync(SocketRequest request)
        {
            var userId = Guid.Parse(request.Data);
            var result = await _userService.EnableUserAsync(userId);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleAssignRoleAsync(SocketRequest request)
        {
            var dto = JsonSerializer.Deserialize<AssignRoleDto>(request.Data);
            var result = await _userService.AssignRoleAsync(dto.UserId, dto.RoleName);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleResetPasswordAsync(SocketRequest request)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(request.Data);
            var userId = Guid.Parse(data["userId"]);
            var newPassword = data["newPassword"];

            var result = await _userService.ResetPasswordAsync(userId, newPassword);
            return SuccessResponse(request.RequestId, result);
        }


        // ==================== ARTIST HANDLERS ====================  quản lí nghệ sĩ
        private async Task<SocketResponse> HandleGetAllArtistsAsync(SocketRequest request)
        {
            var artists = await _artistService.GetAllArtistsAsync();
            return SuccessResponse(request.RequestId, artists);
        }

        private async Task<SocketResponse> HandleCreateArtistAsync(SocketRequest request)
        {
            var createDto = JsonSerializer.Deserialize<CreateArtistDto>(request.Data);

            // TODO: Get userId from token
            var userId = Guid.Empty;

            var artist = await _artistService.CreateArtistAsync(createDto, userId);
            return SuccessResponse(request.RequestId, artist);
        }

        private async Task<SocketResponse> HandleUpdateArtistAsync(SocketRequest request)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(request.Data);
            var artistId = Guid.Parse(data["artistId"].ToString());
            var updateDto = JsonSerializer.Deserialize<UpdateArtistDto>(data["data"].ToString());

            var artist = await _artistService.UpdateArtistAsync(artistId, updateDto);
            return SuccessResponse(request.RequestId, artist);
        }

        private async Task<SocketResponse> HandleDeleteArtistAsync(SocketRequest request)
        {
            var artistId = Guid.Parse(request.Data);
            var result = await _artistService.DeleteArtistAsync(artistId);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleDisableArtistAsync(SocketRequest request)
        {
            var artistId = Guid.Parse(request.Data);
            var result = await _artistService.DisableArtistAsync(artistId);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleEnableArtistAsync(SocketRequest request)
        {
            var artistId = Guid.Parse(request.Data);
            var result = await _artistService.EnableArtistAsync(artistId);
            return SuccessResponse(request.RequestId, result);
        }


        // ==================== SONG HANDLERS ==================== quản lí bài hát
        private async Task<SocketResponse> HandleGetAllSongsAsync(SocketRequest request)
        {
            var songs = await _songService.GetAllSongsAsync("Active", 1, 1000);
            return SuccessResponse(request.RequestId, songs);
        }

        private async Task<SocketResponse> HandleGetPendingSongsAsync(SocketRequest request)
        {
            var songs = await _songService.GetPendingSongsAsync();
            return SuccessResponse(request.RequestId, songs);
        }

        private async Task<SocketResponse> HandleUploadSongFileAsync(SocketRequest request)
        {
            var uploadRequest = JsonSerializer.Deserialize<UploadFileRequest>(request.Data);
            var result = await _fileUploadService.UploadSongFileAsync(uploadRequest);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleCreateSongAsync(SocketRequest request)
        {
            try
            {
                var createDto = JsonSerializer.Deserialize<CreateSongDto>(request.Data);

                // Lấy userId từ helper method
                var userId = await GetCurrentUserIdAsync();

                _logger.LogInformation($" Creating song with userId: {userId}");

                var song = await _songService.CreateSongAsync(createDto, userId);
                return SuccessResponse(request.RequestId, song);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating song");
                return ErrorResponse(request.RequestId, ex.Message);
            }
        }

        private async Task<SocketResponse> HandleUpdateSongAsync(SocketRequest request)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(request.Data);
            var songId = Guid.Parse(data["songId"].ToString());
            var updateDto = JsonSerializer.Deserialize<UpdateSongDto>(data["data"].ToString());

            var song = await _songService.UpdateSongAsync(songId, updateDto);
            return SuccessResponse(request.RequestId, song);
        }

        private async Task<SocketResponse> HandleDeleteSongAsync(SocketRequest request)
        {
            var songId = Guid.Parse(request.Data);
            var result = await _songService.DeleteSongAsync(songId);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleApproveSongAsync(SocketRequest request)
        {
            var songId = Guid.Parse(request.Data);

            // TODO: Get managerId from token
            var managerId = Guid.Empty;

            var result = await _songService.ApproveSongAsync(songId, managerId);
            return SuccessResponse(request.RequestId, result);
        }

        private async Task<SocketResponse> HandleRejectSongAsync(SocketRequest request)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(request.Data);
            var songId = Guid.Parse(data["songId"]);
            var reason = data["reason"];

            // TODO: Get managerId from token
            var managerId = Guid.Empty;

            var result = await _songService.RejectSongAsync(songId, managerId, reason);
            return SuccessResponse(request.RequestId, result);
        }



        private SocketResponse HandleValidateToken(SocketRequest request)
        {
            // TODO: Implement JWT validation
            // Tạm thời return success
            return new SocketResponse
            {
                RequestId = request.RequestId,
                Status = SocketStatus.Success,
                Message = "Token valid",
                Timestamp = DateTime.UtcNow
            };
        }

        private SocketResponse HandleLogout(SocketRequest request)
        {
            return new SocketResponse
            {
                RequestId = request.RequestId,
                Status = SocketStatus.Success,
                Message = "Đăng xuất thành công",
                Timestamp = DateTime.UtcNow
            };
        }



        // ==================== HELPER METHODS ====================
        /// <summary>
        /// Tạo response thành công
        /// </summary>
        private SocketResponse SuccessResponse(Guid requestId, object data)
        {
            return new SocketResponse
            {
                RequestId = requestId,
                Status = SocketStatus.Success,
                Message = "OK",
                Data = JsonSerializer.Serialize(data),
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Tạo response lỗi
        /// </summary>
        private SocketResponse ErrorResponse(Guid requestId, string message)
        {
            return new SocketResponse
            {
                RequestId = requestId,
                Status = SocketStatus.Error,
                Message = message,
                Data = null,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Tạo response unauthorized
        /// </summary>
        private SocketResponse UnauthorizedResponse(Guid requestId, string message = "Unauthorized")
        {
            return new SocketResponse
            {
                RequestId = requestId,
                Status = SocketStatus.Unauthorized,
                Message = message,
                Data = null,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
