using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SystemMonitorUDP.Models;

namespace SystemMonitorUDP.Services
{
    public interface IUdpService
    {
        Task SendDataAsync(SystemMetrics data, string host, int port);
        bool IsValidHost(string host);
    }

    public class UdpService : IUdpService, IDisposable
    {
        private readonly UdpClient _udpClient;
        private readonly JsonSerializerSettings _jsonSettings;
        private bool _disposed = false;

        public UdpService()
        {
            _udpClient = new UdpClient();
            
            // Configure JSON serialization settings to match the desired format
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffffffK"
            };
        }

        public async Task SendDataAsync(SystemMetrics data, string host, int port)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UdpService));
            
            try
            {
                var json = JsonConvert.SerializeObject(data, _jsonSettings);
                var bytes = Encoding.UTF8.GetBytes(json);

                if (IPAddress.TryParse(host, out var ipAddress))
                {
                    await _udpClient.SendAsync(bytes, new IPEndPoint(ipAddress, port));
                }
                else
                {
                    // Try to resolve hostname/mDNS
                    await _udpClient.SendAsync(bytes, host, port);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send UDP data: {ex.Message}", ex);
            }
        }

        public bool IsValidHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return false;

            // Check if it's a valid IP address
            if (IPAddress.TryParse(host, out _))
                return true;

            // Check if it's a valid hostname/mDNS (basic validation)
            return Uri.CheckHostName(host) != UriHostNameType.Unknown;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _udpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}