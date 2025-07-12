# UDP System Monitor

A modern Windows desktop application built with WPF and .NET 9 that monitors key system metrics and transmits them over UDP to remote devices.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)

## ?? Features

### Core Functionality
- **Real-time System Monitoring:**
  - CPU Usage (%)
  - RAM Usage (%)
  - Active Disk Usage (%)
  - Network Speed (MB/s)
  - System Volume Level (%)
  - CPU Temperature (°C)

- **UDP Transmission:** Sends collected metrics as JSON payload to configurable IP address or mDNS name

### User Interface
- **Modern Dark Theme:** Clean, modern UI with dark color scheme
- **Real-time Visualization:** Progress bars for visual representation of system metrics
- **Configuration Panel:** 
  - Target Host (IP address or mDNS name)
  - Port Number (default: 8080)
  - Monitoring Interval in milliseconds (default: 500ms)
- **Control Buttons:** Start/Stop monitoring with real-time status
- **Activity Log:** Timestamped status messages and error reporting
- **Dynamic Window Title:** Reflects application status (Ready/Active)

### System Tray Integration
- **Background Operation:** Runs in system tray when minimized
- **Context Menu:** Show/Hide Window and Exit Application options
- **Double-click Toggle:** Show/hide main window
- **Smart Notifications:** Balloon tips for status changes

### Persistent Settings
- **Auto-start monitoring:** Begin monitoring on application launch
- **Start minimized to tray:** Launch directly to system tray
- **Minimize to tray on close:** Minimize instead of closing when X is clicked
- **Configuration Persistence:** All settings saved across sessions

## ??? Technical Architecture

### Framework & Dependencies
- **.NET 9** with WPF
- **MVVM Architecture** using CommunityToolkit.Mvvm
- **Dependency Injection** with Microsoft.Extensions.DependencyInjection
- **NAudio** for system volume level retrieval
- **System.Management** for WMI queries (CPU temperature)
- **LibreHardwareMonitorLib** for reliable CPU temperature monitoring
- **Newtonsoft.Json** for JSON serialization

### System Monitoring
- **Performance Counters:** CPU, RAM, and disk usage monitoring
- **Network Interface Statistics:** Real-time network speed calculation
- **Audio Endpoint Volume:** System volume level via NAudio
- **Multi-source Temperature:** CPU temperature via multiple sources:
  - LibreHardwareMonitor (primary)
  - MSAcpi_ThermalZoneTemperature (ACPI thermal zones)
  - Win32_TemperatureProbe (Hardware probes)
  - OpenHardwareMonitor WMI (if available)

## ?? JSON Data Format

The application transmits system metrics in the following JSON format:

```json
{
  "CpuUsage": 5.3720236,
  "RamUsage": 94.24391,
  "DiskUsage": 0.3042113,
  "NetworkSpeed": 14.413512727443111,
  "VolumeLevel": 4.0000005,
  "CpuTemperature": 45,
  "Timestamp": "2025-07-12T23:05:37.1851161+05:30"
}
```

**Notes:**
- All numeric values are in double precision
- Network speed is in MB/s (megabytes per second)
- CPU temperature is obtained via multiple monitoring sources for accuracy
- Timestamp includes local timezone offset in ISO 8601 format

## ?? Getting Started

### Prerequisites
- Windows 10/11
- .NET 9 Runtime
- Visual Studio 2022 or later (for development)
- Administrative privileges may be required for some WMI temperature queries

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/udp-system-monitor.git
   cd udp-system-monitor
   ```

2. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

3. **Build the project:**
   ```bash
   dotnet build
   ```

4. **Run the application:**
   ```bash
   dotnet run --project SystemMonitorUDP
   ```

### Usage

1. **Launch Application:** Run SystemMonitorUDP.exe
2. **Configure Target:** Set IP address/hostname and port
3. **Adjust Settings:** Configure monitoring interval and behavior
4. **Start Monitoring:** Click "Start Monitoring" to begin transmission
5. **System Tray:** Minimize to tray for background operation
6. **Monitor Activity:** View real-time status in activity log

## ?? Configuration

Application settings are stored in JSON format at:
`%APPDATA%\SystemMonitorUDP\settings.json`

### Available Settings:
- **TargetHost:** IP address or hostname for UDP transmission
- **Port:** UDP port number (default: 8080)
- **MonitoringInterval:** Data transmission interval in milliseconds (default: 500ms)
- **AutoStartMonitoring:** Automatically start monitoring on application launch
- **StartMinimizedToTray:** Launch application minimized to system tray
- **MinimizeToTrayOnClose:** Minimize to tray instead of closing when X is clicked

## ?? Project Structure

```
SystemMonitorUDP/
??? Models/
?   ??? SystemMetrics.cs      # Data model for metrics
?   ??? AppSettings.cs        # Configuration model
??? Services/
?   ??? SystemMonitorService.cs  # System metrics collection
?   ??? UdpService.cs            # UDP transmission
?   ??? SettingsService.cs      # Configuration persistence
??? ViewModels/
?   ??? MainViewModel.cs        # MVVM view model
??? Views/
?   ??? MainWindow.xaml         # Main application window
?   ??? MainWindow.xaml.cs      # Window code-behind
??? Converters.cs               # Value converters for UI
??? App.xaml.cs                # Application entry point with DI
```

## ?? Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ??? Build Requirements

- Visual Studio 2022 or later
- .NET 9 SDK
- Windows SDK

## ?? Support

If you encounter any issues or have questions, please [open an issue](https://github.com/yourusername/udp-system-monitor/issues) on GitHub.

## ?? Changelog

### v1.0.0
- Initial release
- Real-time system monitoring
- UDP transmission functionality
- Modern WPF UI with system tray integration
- Multi-source CPU temperature monitoring
- Persistent settings management