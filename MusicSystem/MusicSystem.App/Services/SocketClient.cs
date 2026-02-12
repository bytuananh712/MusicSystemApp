using MusicSystem.Shared.SocketContracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MusicSystem.App.Services
{
    public class SocketClient : ISocketClient
    {
        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly string _serverHost;
        private readonly int _serverPort;

        public bool IsConnected => _client?.Connected ?? false;

        public SocketClient(string serverHost = "localhost", int serverPort = 1500)
        {
            _serverHost = serverHost;
            _serverPort = serverPort;
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;

            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_serverHost, _serverPort);

                var stream = _client.GetStream();
                _reader = new StreamReader(stream, Encoding.UTF8);
                _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể kết nối tới server {_serverHost}:{_serverPort}. {ex.Message}");
            }
        }

        public async Task<SocketResponse> SendRequestAsync(SocketRequest request)
        {
            if (!IsConnected)
                await ConnectAsync();

            try
            {
                // Serialize request thành JSON
                var requestJson = JsonSerializer.Serialize(request);

                // Gửi request
                await _writer.WriteLineAsync(requestJson);

                // Đọc response
                var responseJson = await _reader.ReadLineAsync();

                if (string.IsNullOrEmpty(responseJson))
                    throw new Exception("Server đã đóng kết nối");

                // Deserialize response
                var response = JsonSerializer.Deserialize<SocketResponse>(responseJson);
                return response;
            }
            catch (Exception ex)
            {
                Disconnect();
                throw new Exception($"Lỗi giao tiếp với server: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _writer?.Close();
            _reader?.Close();
            _client?.Close();

            _writer = null;
            _reader = null;
            _client = null;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
