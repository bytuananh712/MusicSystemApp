using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MusicSystem.Infrastructure.Socket
{
    public class SocketServer : BackgroundService
    {
        private readonly ILogger<SocketServer> _logger;
        private readonly IServiceProvider _serviceProvider;  // Cho phép tạo ra 1 phạm vi scope mới cho mỗi client
        private TcpListener _listener;
        private const int PORT = 1500;

        public SocketServer(
            ILogger<SocketServer> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, PORT);
                _listener.Start();
                _logger.LogInformation($"✅ Socket Server started on port {PORT}");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var client = await _listener.AcceptTcpClientAsync();
                        var clientEndpoint = client.Client.RemoteEndPoint;
                        _logger.LogInformation($"🔗 Client connected: {clientEndpoint}");

                        // Handle mỗi client trong task riêng
                        _ = Task.Run(async () => await HandleClientAsync(client, stoppingToken), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error accepting client connection");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Socket Server failed to start");
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            // Tạo scope mới cho mỗi client (để DI hoạt động đúng)
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<SocketHandler>();

            try
            {
                await handler.HandleAsync(client, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error handling client");
            }
            finally
            {
                client?.Close();
                _logger.LogInformation("🔌 Client disconnected");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _listener?.Stop();
            _logger.LogInformation("⛔ Socket Server stopped");
            await base.StopAsync(cancellationToken);
        }
    }
}
