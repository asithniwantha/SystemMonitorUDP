// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Suppress IL3000 warnings for Assembly.Location in single-file deployments
// This is expected behavior and handled gracefully in the code
[assembly: SuppressMessage("SingleFile", "IL3000:Avoid accessing Assembly file path when publishing as a single file", 
    Justification = "Assembly.Location is used with proper fallbacks for single-file deployment scenarios")]

// Suppress nullable warnings for platform-specific APIs
[assembly: SuppressMessage("Nullable", "CS8603:Possible null reference return", 
    Scope = "member", Target = "~M:SystemMonitorUDP.Services.StartupService.GetExecutablePath~System.String",
    Justification = "Method has comprehensive null checking and fallback mechanisms")]

// Suppress warnings for Windows Forms interop in WPF application
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", 
    Scope = "member", Target = "~M:SystemMonitorUDP.MainWindow.InitializeSystemTray",
    Justification = "Application is Windows-specific as declared in project file")]