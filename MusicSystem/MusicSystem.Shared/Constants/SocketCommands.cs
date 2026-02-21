using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.Constants
{

//    Constants: là các giá trị cố định dùng chung, tránh hard-code

//    Đặt trong Shared vì:

//    Nhiều layer cùng dùng

//    Không phụ thuộc ngược

//    Giữ kiến trúc sạch

//    Dễ bảo trì
    public static class SocketCommands
    {
        // Authentication
        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string ValidateToken = "ValidateToken";

        // Song Management (dùng sau)
        public const string GetSongs = "GetSongs";
        public const string GetPendingSongs = "GetPendingSongs";
        public const string CreateSong = "CreateSong";
        public const string UpdateSong = "UpdateSong";
        public const string ApproveSong = "ApproveSong";
        public const string DeleteSong = "DeleteSong";

        // Artist Management (Manager only)
        public const string GetAllArtists = "GetAllArtists";
        public const string GetArtistById = "GetArtistById";
        public const string CreateArtist = "CreateArtist";
        public const string UpdateArtist = "UpdateArtist";
        public const string DeleteArtist = "DeleteArtist";
        public const string DisableArtist = "DisableArtist";
        public const string EnableArtist = "EnableArtist";

        // User Management
        public const string GetUsers = "GetUsers";
        public const string CreateUser = "CreateUser";
        public const string AssignRole = "AssignRole";

        // User Management (Admin only)
        public const string GetAllUsers = "GetAllUsers";
        public const string GetUserById = "GetUserById";
       // public const string CreateUser = "CreateUser";
        public const string UpdateUser = "UpdateUser";
        public const string DisableUser = "DisableUser";
        public const string EnableUser = "EnableUser";
       // public const string AssignRole = "AssignRole";
        public const string RemoveRole = "RemoveRole";
        public const string ResetPassword = "ResetPassword";



    }

    public static class SocketStatus
    {
        public const string Success = "Success";
        public const string Error = "Error";
        public const string Unauthorized = "Unauthorized";
        public const string InvalidRequest = "InvalidRequest";
    }

}
