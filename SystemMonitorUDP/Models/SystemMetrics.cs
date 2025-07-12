using Newtonsoft.Json;

namespace SystemMonitorUDP.Models
{
    public class SystemMetrics
    {
        [JsonProperty("CpuUsage")]
        public double CpuUsage { get; set; }

        [JsonProperty("RamUsage")]
        public double RamUsage { get; set; }

        [JsonProperty("DiskUsage")]
        public double DiskUsage { get; set; }

        [JsonProperty("NetworkSpeed")]
        public double NetworkSpeed { get; set; }

        [JsonProperty("VolumeLevel")]
        public double VolumeLevel { get; set; }

        [JsonProperty("CpuTemperature")]
        public double CpuTemperature { get; set; }

        [JsonProperty("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}