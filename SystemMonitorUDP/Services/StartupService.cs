using Microsoft.Win32;
using System.Reflection;
using System.IO;

namespace SystemMonitorUDP.Services
{
    public interface IStartupService
    {
        bool IsStartupEnabled();
        void EnableStartup();
        void DisableStartup();
        void SetStartup(bool enabled);
        string GetStartupInfo(); // Add this for debugging
    }

    public class StartupService : IStartupService
    {
        private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string ApplicationName = "SystemMonitorUDP";

        public bool IsStartupEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                var value = key?.GetValue(ApplicationName);
                return value != null && !string.IsNullOrWhiteSpace(value.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking startup status: {ex.Message}");
                return false;
            }
        }

        public void EnableStartup()
        {
            try
            {
                var executablePath = GetExecutablePath();
                
                if (string.IsNullOrWhiteSpace(executablePath))
                {
                    throw new InvalidOperationException("Could not determine executable path");
                }

                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key != null)
                {
                    key.SetValue(ApplicationName, $"\"{executablePath}\"");
                    System.Diagnostics.Debug.WriteLine($"Startup enabled: {executablePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enabling startup: {ex.Message}");
                throw new InvalidOperationException($"Failed to enable startup: {ex.Message}", ex);
            }
        }

        public void DisableStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key?.GetValue(ApplicationName) != null)
                {
                    key.DeleteValue(ApplicationName);
                    System.Diagnostics.Debug.WriteLine("Startup disabled");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disabling startup: {ex.Message}");
                throw new InvalidOperationException($"Failed to disable startup: {ex.Message}", ex);
            }
        }

        public void SetStartup(bool enabled)
        {
            if (enabled)
            {
                EnableStartup();
            }
            else
            {
                DisableStartup();
            }
        }

        private string GetExecutablePath()
        {
            try
            {
                // Method 1: Try Environment.ProcessPath (.NET 6+)
                var processPath = Environment.ProcessPath;
                if (!string.IsNullOrWhiteSpace(processPath) && File.Exists(processPath))
                {
                    return processPath;
                }

                // Method 2: Try Process.GetCurrentProcess().MainModule.FileName
                using var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                var mainModulePath = currentProcess.MainModule?.FileName;
                if (!string.IsNullOrWhiteSpace(mainModulePath) && File.Exists(mainModulePath))
                {
                    return mainModulePath;
                }

                // Method 3: Try AppContext.BaseDirectory + executable name (.NET Core/5+ recommended)
                var baseDirectory = AppContext.BaseDirectory;
                var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                if (!string.IsNullOrWhiteSpace(assemblyName))
                {
                    var potentialExePath = Path.Combine(baseDirectory, $"{assemblyName}.exe");
                    if (File.Exists(potentialExePath))
                    {
                        return potentialExePath;
                    }
                }

                // Method 4: Try Assembly location (fallback)
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrWhiteSpace(assemblyLocation))
                {
                    // Handle .dll to .exe conversion for .NET apps
                    if (assemblyLocation.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        var exePath = assemblyLocation.Replace(".dll", ".exe");
                        if (File.Exists(exePath))
                        {
                            return exePath;
                        }
                    }
                    
                    if (File.Exists(assemblyLocation))
                    {
                        return assemblyLocation;
                    }
                }

                // Method 5: Try current directory + executable name
                var currentDirectory = Directory.GetCurrentDirectory();
                if (!string.IsNullOrWhiteSpace(assemblyName))
                {
                    var potentialExePath = Path.Combine(currentDirectory, $"{assemblyName}.exe");
                    if (File.Exists(potentialExePath))
                    {
                        return potentialExePath;
                    }
                }

                throw new InvalidOperationException("Could not determine executable path using any method");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting executable path: {ex.Message}");
                throw;
            }
        }

        // Debug method to get detailed startup information
        public string GetStartupInfo()
        {
            try
            {
                var methods = new List<string>();
                
                // Test all methods
                try
                {
                    var processPath = Environment.ProcessPath;
                    methods.Add($"Environment.ProcessPath: {processPath ?? "null"} (Exists: {(!string.IsNullOrWhiteSpace(processPath) ? File.Exists(processPath).ToString() : "N/A")})");
                }
                catch (Exception ex)
                {
                    methods.Add($"Environment.ProcessPath: Error - {ex.Message}");
                }

                try
                {
                    using var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                    var mainModulePath = currentProcess.MainModule?.FileName;
                    methods.Add($"Process.MainModule.FileName: {mainModulePath ?? "null"} (Exists: {(!string.IsNullOrWhiteSpace(mainModulePath) ? File.Exists(mainModulePath).ToString() : "N/A")})");
                }
                catch (Exception ex)
                {
                    methods.Add($"Process.MainModule.FileName: Error - {ex.Message}");
                }

                try
                {
                    var baseDirectory = AppContext.BaseDirectory;
                    var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                    var potentialPath = Path.Combine(baseDirectory, $"{assemblyName}.exe");
                    methods.Add($"AppContext.BaseDirectory: {baseDirectory}");
                    methods.Add($"Assembly Name: {assemblyName}");
                    methods.Add($"Potential EXE Path: {potentialPath} (Exists: {File.Exists(potentialPath)})");
                }
                catch (Exception ex)
                {
                    methods.Add($"AppContext.BaseDirectory: Error - {ex.Message}");
                }

                try
                {
                    var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                    methods.Add($"Assembly.Location: {assemblyLocation ?? "null"} (Exists: {(!string.IsNullOrWhiteSpace(assemblyLocation) ? File.Exists(assemblyLocation).ToString() : "N/A")})");
                }
                catch (Exception ex)
                {
                    methods.Add($"Assembly.Location: Error - {ex.Message}");
                }

                var executablePath = "Unknown";
                try
                {
                    executablePath = GetExecutablePath();
                }
                catch (Exception ex)
                {
                    executablePath = $"Error: {ex.Message}";
                }

                var isEnabled = IsStartupEnabled();
                
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                var registryValue = key?.GetValue(ApplicationName)?.ToString() ?? "Not found";
                
                return $"=== STARTUP DEBUG INFO ===\n" +
                       $"Final Executable Path: {executablePath}\n" +
                       $"Startup Enabled: {isEnabled}\n" +
                       $"Registry Value: {registryValue}\n" +
                       $"Registry Key: HKCU\\{RegistryKeyPath}\\{ApplicationName}\n\n" +
                       $"=== PATH DETECTION METHODS ===\n" +
                       string.Join("\n", methods);
            }
            catch (Exception ex)
            {
                return $"Error getting startup info: {ex.Message}";
            }
        }
    }
}