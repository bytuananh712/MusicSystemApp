using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.Shared.SocketContracts
{
    [Serializable]
    public class SocketResponse
    {
        public Guid RequestId { get; set; } // Match với request
        public string Status { get; set; } // "Success", "Error", "Unauthorized"
        public string Message { get; set; } // Error message hoặc success message
        public string Data { get; set; } // JSON response payload
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
