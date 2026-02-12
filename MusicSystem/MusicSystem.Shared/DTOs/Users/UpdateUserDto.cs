using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Users
{
    public class UpdateUserDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
    }
}
