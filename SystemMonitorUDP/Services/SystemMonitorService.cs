using NAudio.CoreAudioApi;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using SystemMonitorUDP.Models;
using LibreHardwareMonitor.Hardware;

namespace SystemMonitorUDP.Services
{
    public interface ISystemMonitorService
    {
        SystemMetrics GetCurrentMetrics();
        Task<SystemMetrics> GetCurrentMetricsAsync();
    }

    public class SystemMonitorService : ISystemMonitorService, IDisposable
    {
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _ramCounter;
        private readonly PerformanceCounter _diskCounter;
        private NetworkInterface? _primaryNetworkInterface;
        private long _previousBytesReceived;
        private long _previousBytesSent;
        private DateTime _previousTime;
        private Computer? _computer;

        public SystemMonitorService()
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            _diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            
            InitializeNetworkInterface();
            InitializeHardwareMonitor();
            _previousTime = DateTime.Now;
        }

        private void InitializeNetworkInterface()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up && 
                           ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .OrderByDescending(ni => ni.Speed)
                .ToArray();

            _primaryNetworkInterface = interfaces.FirstOrDefault();
        }

        private void InitializeHardwareMonitor()
        {
            try
            {
                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = false,
                    IsMemoryEnabled = false,
                    IsMotherboardEnabled = true,
                    IsControllerEnabled = false,
                    IsNetworkEnabled = false,
                    IsStorageEnabled = false
                };
                _computer.Open();
                System.Diagnostics.Debug.WriteLine("LibreHardwareMonitor initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize LibreHardwareMonitor: {ex.Message}");
                _computer = null;
            }
        }

        public SystemMetrics GetCurrentMetrics()
        {
            return new SystemMetrics
            {
                CpuUsage = GetCpuUsage(),
                RamUsage = GetRamUsage(),
                DiskUsage = GetDiskUsage(),
                NetworkSpeed = GetNetworkSpeed(),
                VolumeLevel = GetVolumeLevel(),
                CpuTemperature = GetCpuTemperature(),
                Timestamp = DateTime.Now
            };
        }

        public async Task<SystemMetrics> GetCurrentMetricsAsync()
        {
            return await Task.Run(() => GetCurrentMetrics());
        }

        private double GetCpuUsage()
        {
            try
            {
                return Math.Round(_cpuCounter.NextValue(), 1);
            }
            catch
            {
                return 0.0;
            }
        }

        private double GetRamUsage()
        {
            try
            {
                var availableRAM = _ramCounter.NextValue();
                var totalRAM = GetTotalPhysicalMemory();
                var usedRAM = totalRAM - availableRAM;
                return Math.Round((usedRAM / totalRAM) * 100, 1);
            }
            catch
            {
                return 0.0;
            }
        }

        private double GetTotalPhysicalMemory()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                using var results = searcher.Get();
                var totalBytes = Convert.ToDouble(results.Cast<ManagementObject>().First()["TotalPhysicalMemory"]);
                return totalBytes / (1024 * 1024); // Convert to MB
            }
            catch
            {
                return 8192; // Default fallback
            }
        }

        private double GetDiskUsage()
        {
            try
            {
                return Math.Round(_diskCounter.NextValue(), 1);
            }
            catch
            {
                return 0.0;
            }
        }

        private double GetNetworkSpeed()
        {
            try
            {
                if (_primaryNetworkInterface == null)
                {
                    InitializeNetworkInterface();
                    return 0.0;
                }

                var stats = _primaryNetworkInterface.GetIPv4Statistics();
                var currentBytesReceived = stats.BytesReceived;
                var currentBytesSent = stats.BytesSent;
                var currentTime = DateTime.Now;

                var timeDiff = (currentTime - _previousTime).TotalSeconds;
                if (timeDiff > 0 && _previousBytesReceived > 0)
                {
                    var bytesDiff = (currentBytesReceived + currentBytesSent) - (_previousBytesReceived + _previousBytesSent);
                    var speed = (bytesDiff / timeDiff) / (1024 * 1024); // Convert to MB/s
                    
                    _previousBytesReceived = currentBytesReceived;
                    _previousBytesSent = currentBytesSent;
                    _previousTime = currentTime;
                    
                    return Math.Round(Math.Max(0, speed), 3);
                }

                _previousBytesReceived = currentBytesReceived;
                _previousBytesSent = currentBytesSent;
                _previousTime = currentTime;
                return 0.0;
            }
            catch
            {
                return 0.0;
            }
        }

        private double GetVolumeLevel()
        {
            try
            {
                using var deviceEnumerator = new MMDeviceEnumerator();
                var defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                return Math.Round(defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100.0, 7);
            }
            catch
            {
                return 0.0;
            }
        }

        private double GetCpuTemperature()
        {
            try
            {
                // Try LibreHardwareMonitor first (most reliable)
                var libreTemp = GetTemperatureFromLibreHardware();
                if (libreTemp.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"LibreHardware Temperature: {libreTemp.Value}°C");
                    return libreTemp.Value;
                }

                // Try multiple WMI queries for temperature with detailed logging
                var acpiTemp = GetTemperatureFromACPI();
                if (acpiTemp.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"ACPI Temperature: {acpiTemp.Value}°C");
                    return acpiTemp.Value;
                }

                var probeTemp = GetTemperatureFromProbe();
                if (probeTemp.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"Probe Temperature: {probeTemp.Value}°C");
                    return probeTemp.Value;
                }

                var openHwTemp = GetTemperatureFromOpenHardware();
                if (openHwTemp.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"OpenHardware Temperature: {openHwTemp.Value}°C");
                    return openHwTemp.Value;
                }

                // Try Windows Performance Counters as additional fallback
                var perfCounterTemp = GetTemperatureFromPerformanceCounters();
                if (perfCounterTemp.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"Performance Counter Temperature: {perfCounterTemp.Value}°C");
                    return perfCounterTemp.Value;
                }

                System.Diagnostics.Debug.WriteLine("No temperature source available, using fallback");
                return 45.0; // Default fallback temperature
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Temperature error: {ex.Message}");
                return 45.0;
            }
        }

        private double? GetTemperatureFromLibreHardware()
        {
            try
            {
                if (_computer == null) return null;

                _computer.Accept(new UpdateVisitor());

                foreach (var hardware in _computer.Hardware)
                {
                    if (hardware.HardwareType == HardwareType.Cpu)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature && 
                                sensor.Name.Contains("CPU") && 
                                sensor.Value.HasValue)
                            {
                                var temp = sensor.Value.Value;
                                System.Diagnostics.Debug.WriteLine($"LibreHardware CPU temp: {temp}°C from {sensor.Name}");
                                
                                if (temp >= 10 && temp <= 100)
                                {
                                    return Math.Round(temp, 1);
                                }
                            }
                        }
                    }
                    else if (hardware.HardwareType == HardwareType.Motherboard)
                    {
                        foreach (var subHardware in hardware.SubHardware)
                        {
                            foreach (var sensor in subHardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Temperature && 
                                    sensor.Name.Contains("CPU") && 
                                    sensor.Value.HasValue)
                                {
                                    var temp = sensor.Value.Value;
                                    System.Diagnostics.Debug.WriteLine($"LibreHardware motherboard CPU temp: {temp}°C from {sensor.Name}");
                                    
                                    if (temp >= 10 && temp <= 100)
                                    {
                                        return Math.Round(temp, 1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LibreHardware temp error: {ex.Message}");
            }
            return null;
        }

        private double? GetTemperatureFromACPI()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                using var results = searcher.Get();
                
                foreach (var result in results.Cast<ManagementObject>())
                {
                    if (result["CurrentTemperature"] != null)
                    {
                        var rawTemp = Convert.ToDouble(result["CurrentTemperature"]);
                        System.Diagnostics.Debug.WriteLine($"Raw ACPI temp: {rawTemp}");
                        
                        // MSAcpi_ThermalZoneTemperature returns temperature in tenths of Kelvin
                        var tempKelvin = rawTemp / 10.0;
                        var tempCelsius = tempKelvin - 273.15;
                        
                        System.Diagnostics.Debug.WriteLine($"ACPI: {rawTemp} raw -> {tempKelvin}K -> {tempCelsius}°C");
                        
                        // Sanity check: CPU temp should be between 10°C and 100°C
                        if (tempCelsius >= 10 && tempCelsius <= 100)
                        {
                            return Math.Round(tempCelsius, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ACPI temp error: {ex.Message}");
            }
            return null;
        }

        private double? GetTemperatureFromProbe()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_TemperatureProbe");
                using var results = searcher.Get();
                
                foreach (var result in results.Cast<ManagementObject>())
                {
                    if (result["CurrentReading"] != null)
                    {
                        var rawTemp = Convert.ToDouble(result["CurrentReading"]);
                        System.Diagnostics.Debug.WriteLine($"Raw probe temp: {rawTemp}");
                        
                        // Win32_TemperatureProbe can return in different units, try different conversions
                        double tempCelsius;
                        
                        // If it's a large number, it might be in tenths of Kelvin
                        if (rawTemp > 1000)
                        {
                            tempCelsius = (rawTemp / 10.0) - 273.15;
                        }
                        // If it's already in reasonable range, it might be Celsius
                        else if (rawTemp > 10 && rawTemp < 100)
                        {
                            tempCelsius = rawTemp;
                        }
                        // If it's in Kelvin range
                        else if (rawTemp > 273 && rawTemp < 373)
                        {
                            tempCelsius = rawTemp - 273.15;
                        }
                        else
                        {
                            continue; // Skip this reading
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"Probe: {rawTemp} raw -> {tempCelsius}°C");
                        
                        // Sanity check
                        if (tempCelsius >= 10 && tempCelsius <= 100)
                        {
                            return Math.Round(tempCelsius, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Probe temp error: {ex.Message}");
            }
            return null;
        }

        private double? GetTemperatureFromOpenHardware()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\OpenHardwareMonitor", "SELECT * FROM Sensor WHERE SensorType='Temperature' AND Name LIKE '%CPU%'");
                using var results = searcher.Get();
                
                foreach (var result in results.Cast<ManagementObject>())
                {
                    if (result["Value"] != null)
                    {
                        var temp = Convert.ToDouble(result["Value"]);
                        System.Diagnostics.Debug.WriteLine($"OpenHardware temp: {temp}°C");
                        
                        // OpenHardwareMonitor typically returns temperature in Celsius
                        if (temp >= 10 && temp <= 100)
                        {
                            return Math.Round(temp, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OpenHardware temp error: {ex.Message}");
            }
            return null;
        }

        private double? GetTemperatureFromPerformanceCounters()
        {
            try
            {
                // Try to get temperature from performance counters
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PerfRawData_Counters_ThermalZoneInformation");
                using var results = searcher.Get();
                
                foreach (var result in results.Cast<ManagementObject>())
                {
                    if (result["Temperature"] != null)
                    {
                        var rawTemp = Convert.ToDouble(result["Temperature"]);
                        System.Diagnostics.Debug.WriteLine($"Raw perf counter temp: {rawTemp}");
                        
                        // This is typically in tenths of Kelvin
                        var tempCelsius = (rawTemp / 10.0) - 273.15;
                        
                        System.Diagnostics.Debug.WriteLine($"PerfCounter: {rawTemp} raw -> {tempCelsius}°C");
                        
                        if (tempCelsius >= 10 && tempCelsius <= 100)
                        {
                            return Math.Round(tempCelsius, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Performance Counter temp error: {ex.Message}");
            }
            return null;
        }

        public void Dispose()
        {
            _cpuCounter?.Dispose();
            _ramCounter?.Dispose();
            _diskCounter?.Dispose();
            _computer?.Close();
            _computer = null;
        }
    }

    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware)
                subHardware.Accept(this);
        }

        public void VisitSensor(ISensor sensor) { }

        public void VisitParameter(IParameter parameter) { }
    }
}