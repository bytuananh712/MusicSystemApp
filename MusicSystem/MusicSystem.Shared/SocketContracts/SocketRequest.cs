using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.SocketContracts
{
    [Serializable]
    public class SocketRequest
    {
        public Guid RequestId { get; set; } = Guid.NewGuid();
        public string Command { get; set; } // "Login", "GetSongs", "ApproveSong", etc.
        public string Token { get; set; } // Authentication token
        public string Data { get; set; } // JSON payload
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}


//SocketContracts = tập các lớp(đối tượng) dùng để ĐÓNG GÓI DỮ LIỆU
//và TRAO ĐỔI qua lại giữa client ↔ server bằng TCP (socket).