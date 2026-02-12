using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.DTOs.Users
{
    public class AssignRoleDto
    {
        public Guid UserId { get; set; }
        public string RoleName { get; set; } // "Admin", "Manager", "Customer"
    }
}
