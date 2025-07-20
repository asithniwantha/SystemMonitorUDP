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
        
        // Retry mechanism settings
        public bool EnableRetries { get; set; } = true;
        public int MaxRetries { get; set; } = 2;
        public int BaseDelayMs { get; set; } = 100;
        
        // UI Settings
        public bool AutoScrollActivityLog { get; set; } = true;
        
        // Host resolution and auto-restart settings
        public bool AutoRestartOnHostFailure { get; set; } = true;
        public int HostResolutionTimeoutSeconds { get; set; } = 30;
        public int MaxConsecutiveFailures { get; set; } = 5;
        public int RestartDelaySeconds { get; set; } = 10;
    }
}