using System.Drawing;
using System.IO;
using System.Reflection;

namespace SystemMonitorUDP.Services
{
    public interface IIconService
    {
        Icon GetApplicationIcon();
        Icon GetTrayIcon();
        bool HasCustomIcon();
    }

    public class IconService : IIconService, IDisposable
    {
        private Icon? _applicationIcon;
        private Icon? _trayIcon;

        public Icon GetApplicationIcon()
        {
            _applicationIcon ??= LoadIcon("app.ico") ?? CreateDefaultIcon();
            return _applicationIcon;
        }

        public Icon GetTrayIcon()
        {
            _trayIcon ??= LoadIcon("tray.ico") ?? GetApplicationIcon();
            return _trayIcon;
        }

        public bool HasCustomIcon()
        {
            return LoadIcon("app.ico") != null;
        }

        private Icon? LoadIcon(string iconName)
        {
            try
            {
                // Try to load from Resources folder
                var resourcePath = Path.Combine(AppContext.BaseDirectory, "Resources", iconName);
                if (File.Exists(resourcePath))
                {
                    return new Icon(resourcePath);
                }

                // Try to load from embedded resources
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"SystemMonitorUDP.Resources.{iconName}";
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    return new Icon(stream);
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading icon {iconName}: {ex.Message}");
                return null;
            }
        }

        private Icon CreateDefaultIcon()
        {
            try
            {
                // Create a simple programmatic icon as fallback
                using var bitmap = new Bitmap(32, 32);
                using var graphics = Graphics.FromImage(bitmap);

                // Fill background
                using var backgroundBrush = new SolidBrush(Color.FromArgb(45, 45, 48)); // #2D2D30
                graphics.FillEllipse(backgroundBrush, 2, 2, 28, 28);

                // Draw simple chart bars
                using var barBrush = new SolidBrush(Color.FromArgb(14, 99, 156)); // #0E639C
                graphics.FillRectangle(barBrush, 8, 20, 3, 8);
                graphics.FillRectangle(barBrush, 12, 16, 3, 12);
                graphics.FillRectangle(barBrush, 16, 12, 3, 16);
                graphics.FillRectangle(barBrush, 20, 14, 3, 14);

                // Draw signal waves
                using var wavePen = new Pen(Color.White, 1);
                graphics.DrawArc(wavePen, 22, 6, 6, 6, -45, 90);
                graphics.DrawArc(wavePen, 20, 4, 10, 10, -45, 90);

                // Convert bitmap to icon
                var iconHandle = bitmap.GetHicon();
                return Icon.FromHandle(iconHandle);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating default icon: {ex.Message}");
                return SystemIcons.Application;
            }
        }

        public void Dispose()
        {
            _applicationIcon?.Dispose();
            _trayIcon?.Dispose();
            _applicationIcon = null;
            _trayIcon = null;
        }
    }
}