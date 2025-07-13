using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using SystemMonitorUDP.Models;
using SystemMonitorUDP.Services;

namespace SystemMonitorUDP.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ISystemMonitorService _systemMonitorService;
        private readonly IUdpService _udpService;
        private readonly ISettingsService _settingsService;
        private readonly IStartupService _startupService;
        private readonly System.Timers.Timer _monitoringTimer;
        private int _debugCounter = 0;

        [ObservableProperty]
        private double _cpuUsage;

        [ObservableProperty]
        private double _ramUsage;

        [ObservableProperty]
        private double _diskUsage;

        [ObservableProperty]
        private double _networkSpeed;

        [ObservableProperty]
        private double _volumeLevel;

        [ObservableProperty]
        private double _cpuTemperature;

        [ObservableProperty]
        private string _targetHost = "127.0.0.1";

        [ObservableProperty]
        private int _port = 8080;

        [ObservableProperty]
        private int _monitoringInterval = 500;

        [ObservableProperty]
        private bool _autoStartMonitoring;

        [ObservableProperty]
        private bool _startMinimizedToTray;

        [ObservableProperty]
        private bool _minimizeToTrayOnClose = true;

        [ObservableProperty]
        private bool _startWithWindows;

        [ObservableProperty]
        private bool _isMonitoring;

        [ObservableProperty]
        private string _windowTitle = "UDP System Monitor - Ready";

        public ObservableCollection<string> ActivityLog { get; } = new();

        public MainViewModel(ISystemMonitorService systemMonitorService, IUdpService udpService, ISettingsService settingsService, IStartupService startupService)
        {
            _systemMonitorService = systemMonitorService;
            _udpService = udpService;
            _settingsService = settingsService;
            _startupService = startupService;

            _monitoringTimer = new System.Timers.Timer();
            _monitoringTimer.Elapsed += MonitoringTimer_Elapsed;

            LoadSettings();
            AddLogEntry("Application started");
        }

        [RelayCommand]
        public async Task StartMonitoring()
        {
            if (IsMonitoring) return;

            if (!_udpService.IsValidHost(TargetHost))
            {
                AddLogEntry("Error: Invalid target host");
                return;
            }

            IsMonitoring = true;
            WindowTitle = "UDP System Monitor - Active";
            
            _monitoringTimer.Interval = MonitoringInterval;
            _monitoringTimer.Start();
            
            AddLogEntry($"Monitoring started - Target: {TargetHost}:{Port}, Interval: {MonitoringInterval}ms");
            await SaveSettings();
        }

        [RelayCommand]
        public async Task StopMonitoring()
        {
            if (!IsMonitoring) return;

            IsMonitoring = false;
            WindowTitle = "UDP System Monitor - Ready";
            
            _monitoringTimer.Stop();
            AddLogEntry("Monitoring stopped");
            await SaveSettings();
        }

        [RelayCommand]
        public async Task SaveSettings()
        {
            var settings = new AppSettings
            {
                TargetHost = TargetHost,
                Port = Port,
                MonitoringInterval = MonitoringInterval,
                AutoStartMonitoring = AutoStartMonitoring,
                StartMinimizedToTray = StartMinimizedToTray,
                MinimizeToTrayOnClose = MinimizeToTrayOnClose,
                StartWithWindows = StartWithWindows
            };

            await _settingsService.SaveSettingsAsync(settings);
            
            // Handle Windows startup setting
            try
            {
                _startupService.SetStartup(StartWithWindows);
                AddLogEntry($"Windows startup {(StartWithWindows ? "enabled" : "disabled")}");
            }
            catch (Exception ex)
            {
                AddLogEntry($"Error updating startup setting: {ex.Message}");
            }
            
            AddLogEntry("Settings saved");
        }

        [RelayCommand]
        public void DebugStartup()
        {
            try
            {
                var debugInfo = _startupService.GetStartupInfo();
                var lines = debugInfo.Split('\n');
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        AddLogEntry($"DEBUG: {line}");
                    }
                }
            }
            catch (Exception ex)
            {
                AddLogEntry($"Debug error: {ex.Message}");
            }
        }

        private async void MonitoringTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var metrics = await _systemMonitorService.GetCurrentMetricsAsync();
                
                // Update UI on main thread
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CpuUsage = metrics.CpuUsage;
                    RamUsage = metrics.RamUsage;
                    DiskUsage = metrics.DiskUsage;
                    NetworkSpeed = metrics.NetworkSpeed;
                    VolumeLevel = metrics.VolumeLevel;
                    CpuTemperature = metrics.CpuTemperature;
                });

                await _udpService.SendDataAsync(metrics, TargetHost, Port);
                
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    AddLogEntry($"Data sent - CPU: {metrics.CpuUsage:F1}%, RAM: {metrics.RamUsage:F1}%, Vol: {metrics.VolumeLevel:F1}%, Temp: {metrics.CpuTemperature:F1}°C");
                    
                    // Add debug info for temperature every 10 seconds (20 intervals at 500ms)
                    if (_debugCounter++ % 20 == 0)
                    {
                        AddLogEntry($"Debug - Raw temp data check: {metrics.CpuTemperature:F1}°C");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    AddLogEntry($"Error: {ex.Message}");
                });
            }
        }

        private void LoadSettings()
        {
            var settings = _settingsService.LoadSettings();
            TargetHost = settings.TargetHost;
            Port = settings.Port;
            MonitoringInterval = settings.MonitoringInterval;
            AutoStartMonitoring = settings.AutoStartMonitoring;
            StartMinimizedToTray = settings.StartMinimizedToTray;
            MinimizeToTrayOnClose = settings.MinimizeToTrayOnClose;
            
            // Load startup setting and sync with registry
            StartWithWindows = settings.StartWithWindows;
            
            // Verify actual startup status matches settings
            try
            {
                var actualStartupStatus = _startupService.IsStartupEnabled();
                if (actualStartupStatus != StartWithWindows)
                {
                    StartWithWindows = actualStartupStatus;
                    AddLogEntry($"Startup setting synchronized: {(StartWithWindows ? "enabled" : "disabled")}");
                }
            }
            catch (Exception ex)
            {
                AddLogEntry($"Warning: Could not check startup status: {ex.Message}");
            }
        }

        private void AddLogEntry(string message)
        {
            var entry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            ActivityLog.Add(entry);
            
            // Keep only last 100 entries
            while (ActivityLog.Count > 100)
            {
                ActivityLog.RemoveAt(0);
            }
        }

        public async Task InitializeAsync()
        {
            if (AutoStartMonitoring)
            {
                await StartMonitoring();
            }
        }

        public void Dispose()
        {
            _monitoringTimer?.Dispose();
        }
    }
}