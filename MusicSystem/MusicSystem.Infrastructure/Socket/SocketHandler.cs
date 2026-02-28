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
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MusicSystem.Infrastructure.Socket
{
    public class SocketHandler
    {
        private readonly ILogger<SocketHandler> _logger;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IArtistService _artistService;
        private readonly ISongService _songService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IUserRepository _userRepository;

        public SocketHandler(
            ILogger<SocketHandler> logger,
            IAuthService authService,
            IUserService userService,
            IArtistService artistService,
            ISongService songService,
            IFileUploadService fileUploadService,
            IUserRepository userRepository)
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
            var clientEndpoint = client.Client.RemoteEndPoint?.ToString();
            _logger.LogInformation(" Client connected: {ClientEndpoint}", clientEndpoint);

            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                try
                {
                    var requestJson = await reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(requestJson))
                    {
                        _logger.LogWarning(" Empty request received");
                        break;
                    }

                    //  CHỈ log JSON ở Debug level
                    _logger.LogDebug(" Received: {RequestJson}", requestJson);

                    var request = JsonSerializer.Deserialize<SocketRequest>(requestJson);

                    //  Log summary - ngắn gọn
                    _logger.LogInformation(" Command: {Command}", request.Command);

                    var response = await ProcessRequestAsync(request);

                    var responseJson = JsonSerializer.Serialize(response);
                    await writer.WriteLineAsync(responseJson);

                    //  Log summary - ngắn gọn
                    _logger.LogInformation(" Status: {Status}", response.Status);

                    //  CHỈ log JSON ở Debug level
                    _logger.LogDebug(" Sent: {ResponseJson}", responseJson);
                }
                catch (IOException ioEx)
                {
                    _logger.LogWarning(ioEx, " Client disconnected");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, " Error processing request");

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

            _logger.LogInformation(" Client disconnected: {ClientEndpoint}", clientEndpoint);
        }

        private async Task<SocketResponse> ProcessRequestAsync(SocketRequest request)
        {
            try
            {
                return request.Command switch
                {
                    SocketCommands.Login => await HandleLoginAsync(request),
                    SocketCommands.ValidateToken => HandleValidateToken(request),
                    SocketCommands.Logout => HandleLogout(request),

                    // Admin
                    SocketCommands.GetAllUsers => await HandleGetAllUsersAsync(request),
                    SocketCommands.CreateUser => await HandleCreateUserAsync(request),
                    SocketCommands.UpdateUser => await HandleUpdateUserAsync(request),
                    SocketCommands.DisableUser => await HandleDisableUserAsync(request),
                    SocketCommands.EnableUser => await HandleEnableUserAsync(request),
                    SocketCommands.AssignRole => await HandleAssignRoleAsync(request),
                    SocketCommands.ResetPassword => await HandleResetPasswordAsync(request),

                    // Artist
                    SocketCommands.GetAllArtists => await HandleGetAllArtistsAsync(request),
                    SocketCommands.CreateArtist => await HandleCreateArtistAsync(request),
                    SocketCommands.UpdateArtist => await HandleUpdateArtistAsync(request),
                    SocketCommands.DeleteArtist => await HandleDeleteArtistAsync(request),
                    SocketCommands.DisableArtist => await HandleDisableArtistAsync(request),
                    SocketCommands.EnableArtist => await HandleEnableArtistAsync(request),

                    // Song
                    SocketCommands.GetAllSongs => await HandleGetAllSongsAsync(request),
                    SocketCommands.GetPendingSongs => await HandleGetPendingSongsAsync(request),
                    SocketCommands.UploadSongFile => await HandleUploadSongFileAsync(request),
                    SocketCommands.CreateSong => await HandleCreateSongAsync(request),
                    SocketCommands.UpdateSong => await HandleUpdateSongAsync(request),
                    SocketCommands.DeleteSong => await HandleDeleteSongAsync(request),
                    SocketCommands.ApproveSong => await HandleApproveSongAsync(request),
                    SocketCommands.RejectSong => await HandleRejectSongAsync(request),

                    _ => new SocketResponse
                    {
                        RequestId = request.RequestId,
                        Status = SocketStatus.InvalidRequest,
                        Message = $"Unknown command: {request.Command}",
                        Timestamp = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Command error: {Command}", request.Command);
                return ErrorResponse(request.RequestId, ex.Message);
            }
        }

        // ==================== AUTH HANDLERS ====================

        private async Task<SocketResponse> HandleLoginAsync(SocketRequest request)
        {
            try
            {
                var loginRequest = JsonSerializer.Deserialize<LoginRequestDto>(request.Data);

                _logger.LogInformation(" Login attempt: {Username}", loginRequest.Username);

                var loginResult = await _authService.LoginAsync(loginRequest);

                if (!loginResult.Success)
                {
                    _logger.LogWarning(" Login failed: {Message}", loginResult.Message);
                    return UnauthorizedResponse(request.RequestId, loginResult.Message);
                }

                if (!loginResult.User.Roles.Contains("Admin") && !loginResult.User.Roles.Contains("Manager"))
                {
                    _logger.LogWarning(" Unauthorized role: {Roles}", string.Join(", ", loginResult.User.Roles));
                    return UnauthorizedResponse(request.RequestId, "Chỉ Admin và Manager mới có thể đăng nhập vào ứng dụng quản trị");
                }

                _logger.LogInformation(" Login successful: {Username} ({Roles})",
                    loginResult.User.Username, string.Join(", ", loginResult.User.Roles));

                return SuccessResponse(request.RequestId, loginResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Login error");
                return ErrorResponse(request.RequestId, $"Lỗi đăng nhập: {ex.Message}");
            }
        }

        // ==================== ARTIST HANDLERS ====================

        private async Task<SocketResponse> HandleGetAllArtistsAsync(SocketRequest request)
        {
            var artists = await _artistService.GetAllArtistsAsync();
            var artistList = artists.ToList();

            _logger.LogInformation(" Found {Count} artists", artistList.Count);

            return SuccessResponse(request.RequestId, artistList);
        }

        private async Task<SocketResponse> HandleCreateArtistAsync(SocketRequest request)
        {
            var createDto = JsonSerializer.Deserialize<CreateArtistDto>(request.Data);

            _logger.LogInformation(" Creating artist: {ArtistName}", createDto.ArtistName);

            var artist = await _artistService.CreateArtistAsync(createDto);

            _logger.LogInformation(" Artist created: {ArtistName}", artist.ArtistName);

            return SuccessResponse(request.RequestId, artist);
        }

        // ==================== SONG HANDLERS ====================

        private async Task<SocketResponse> HandleGetPendingSongsAsync(SocketRequest request)
        {
            var songs = await _songService.GetPendingSongsAsync();
            var songList = songs.ToList();

            _logger.LogInformation(" Found {Count} pending songs", songList.Count);

            return SuccessResponse(request.RequestId, songList);
        }

        private async Task<SocketResponse> HandleCreateSongAsync(SocketRequest request)
        {
            try
            {
                var createDto = JsonSerializer.Deserialize<CreateSongDto>(request.Data);
                var userId = await GetCurrentUserIdAsync();

                _logger.LogInformation(" Creating song: {Title}", createDto.Title);

                var song = await _songService.CreateSongAsync(createDto, userId);

                _logger.LogInformation(" Song created: {Title}", song.Title);

                return SuccessResponse(request.RequestId, song);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error creating song");
                return ErrorResponse(request.RequestId, ex.Message);
            }
        }

        private async Task<SocketResponse> HandleApproveSongAsync(SocketRequest request)
        {
            var songId = Guid.Parse(request.Data);
            var adminId = await GetCurrentUserIdAsync();

            _logger.LogInformation(" Approving song: {SongId}", songId);

            var result = await _songService.ApproveSongAsync(songId, adminId);

            _logger.LogInformation(" Song approved");

            return SuccessResponse(request.RequestId, result);
        }

        // ==================== HELPER METHODS ====================

        private async Task<Guid> GetCurrentUserIdAsync()
        {
            var users = await _userRepository.GetAllWithRolesAsync();
            var user = users.FirstOrDefault(u =>
                u.UserRoles.Any(ur => ur.Role.RoleName == "Manager" || ur.Role.RoleName == "Admin"));

            if (user == null)
                throw new Exception("No valid Manager or Admin user found");

            return user.UserId;
        }

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

        private SocketResponse ErrorResponse(Guid requestId, string message)
        {
            return new SocketResponse
            {
                RequestId = requestId,
                Status = SocketStatus.Error,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        private SocketResponse UnauthorizedResponse(Guid requestId, string message)
        {
            return new SocketResponse
            {
                RequestId = requestId,
                Status = SocketStatus.Unauthorized,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        private SocketResponse HandleValidateToken(SocketRequest request)
        {
            return SuccessResponse(request.RequestId, new { valid = true });
        }

        private SocketResponse HandleLogout(SocketRequest request)
        {
            _logger.LogInformation(" Logout");
            return SuccessResponse(request.RequestId, new { success = true });
        }

        // ==================== IMPLEMENT MISSING HANDLERS ====================

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

        private async Task<SocketResponse> HandleGetAllSongsAsync(SocketRequest request)
        {
            var songs = await _songService.GetAllSongsAsync("Active", 1, 1000);
            return SuccessResponse(request.RequestId, songs);
        }

        private async Task<SocketResponse> HandleUploadSongFileAsync(SocketRequest request)
        {
            var uploadRequest = JsonSerializer.Deserialize<UploadFileRequest>(request.Data);
            var result = await _fileUploadService.UploadSongFileAsync(uploadRequest);
            return SuccessResponse(request.RequestId, result);
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

        private async Task<SocketResponse> HandleRejectSongAsync(SocketRequest request)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(request.Data);
            var songId = Guid.Parse(data["songId"]);
            var reason = data.ContainsKey("reason") ? data["reason"] : "Không đạt yêu cầu";
            var adminId = await GetCurrentUserIdAsync();
            var result = await _songService.RejectSongAsync(songId, adminId, reason);
            return SuccessResponse(request.RequestId, result);
        }
    }
}