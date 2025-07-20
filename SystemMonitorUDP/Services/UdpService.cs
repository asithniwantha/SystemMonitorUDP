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
        Task SendDataWithRetryAsync(SystemMetrics data, string host, int port, int maxRetries = 3, TimeSpan? baseDelay = null);
        bool IsValidHost(string host);
        Task<bool> CanResolveHostAsync(string host);
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

        public async Task SendDataWithRetryAsync(SystemMetrics data, string host, int port, int maxRetries = 3, TimeSpan? baseDelay = null)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UdpService));

            var delay = baseDelay ?? TimeSpan.FromMilliseconds(100); // Default 100ms base delay
            var lastException = new Exception("Unknown error");

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
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
                    
                    // Success - exit the retry loop
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    // If this is the last attempt, don't wait
                    if (attempt == maxRetries)
                        break;

                    // Calculate exponential backoff delay
                    var currentDelay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * Math.Pow(2, attempt));
                    
                    // Add some jitter to prevent thundering herd (±25% randomization)
                    var jitter = Random.Shared.NextDouble() * 0.5 - 0.25; // -0.25 to +0.25
                    var jitteredDelay = TimeSpan.FromMilliseconds(currentDelay.TotalMilliseconds * (1 + jitter));
                    
                    // Cap the maximum delay to 5 seconds
                    var finalDelay = jitteredDelay.TotalMilliseconds > 5000 
                        ? TimeSpan.FromMilliseconds(5000) 
                        : jitteredDelay;

                    await Task.Delay(finalDelay);
                }
            }

            // All retries failed, throw the last exception
            throw new InvalidOperationException(
                $"Failed to send UDP data after {maxRetries + 1} attempts. Last error: {lastException.Message}", 
                lastException);
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

        public async Task<bool> CanResolveHostAsync(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return false;

            try
            {
                // If it's already an IP address, it's resolvable
                if (IPAddress.TryParse(host, out _))
                    return true;

                // Try to resolve the hostname
                var hostEntry = await Dns.GetHostEntryAsync(host);
                return hostEntry.AddressList.Length > 0;
            }
            catch (SocketException)
            {
                // Host not found or network error
                return false;
            }
            catch (Exception)
            {
                // Other DNS resolution errors
                return false;
            }
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