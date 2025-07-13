namespace SystemMonitorUDP.Models
{
    public class AppSettings
    {
        public string TargetHost { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 8080;
        public int MonitoringInterval { get; set; } = 500;
        public bool AutoStartMonitoring { get; set; } = false;
        public bool StartMinimizedToTray { get; set; } = false;
        public bool MinimizeToTrayOnClose { get; set; } = true;
        public bool StartWithWindows { get; set; } = false;
    }
}