using MusicSystem.Shared.SocketContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSystem.App.Services
{
    public interface ISocketClient : IDisposable
    {
        bool IsConnected { get; }
        Task ConnectAsync();
        Task<SocketResponse> SendRequestAsync(SocketRequest request);
        void Disconnect();
    }
}
