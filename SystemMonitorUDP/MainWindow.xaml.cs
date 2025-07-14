using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SystemMonitorUDP.Services;
using SystemMonitorUDP.ViewModels;

namespace SystemMonitorUDP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon? _notifyIcon;
        private bool _isClosing = false;
        private readonly MainViewModel _viewModel;
        private readonly ISettingsService _settingsService;
        private readonly IIconService _iconService;

        public MainWindow(MainViewModel viewModel, ISettingsService settingsService, IIconService iconService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _settingsService = settingsService;
            _iconService = iconService;
            DataContext = _viewModel;

            InitializeSystemTray();
            InitializeAutoScroll();

            // Handle window state changes
            this.StateChanged += MainWindow_StateChanged;
            this.Closing += MainWindow_Closing;
            this.Loaded += MainWindow_Loaded;
        }

        private void InitializeAutoScroll()
        {
            // Subscribe to the ActivityLogUpdated event for auto-scroll functionality
            _viewModel.ActivityLogUpdated += OnActivityLogUpdated;
        }

        private void OnActivityLogUpdated(object? sender, EventArgs e)
        {
            // Ensure we're on the UI thread
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    // Find the ListView by name
                    var listView = this.FindName("ActivityLogListView") as System.Windows.Controls.ListView;
                    if (listView != null && listView.Items.Count > 0)
                    {
                        var lastItem = listView.Items[listView.Items.Count - 1];
                        listView.ScrollIntoView(lastItem);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't crash the application
                    System.Diagnostics.Debug.WriteLine($"Failed to auto-scroll activity log: {ex.Message}");
                }
            });
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = await _settingsService.LoadSettingsAsync();

            if (settings.StartMinimizedToTray)
            {
                this.WindowState = WindowState.Minimized;
                this.Hide();
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = true;
                }
            }

            await _viewModel.InitializeAsync();
        }

        private void InitializeSystemTray()
        {
            try
            {
                _notifyIcon = new NotifyIcon();

                // Use IconService for custom icon, fallback to system icon
                try
                {
                    _notifyIcon.Icon = _iconService.GetTrayIcon();
                }
                catch
                {
                    _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                }

                _notifyIcon.Text = "UDP System Monitor";
                _notifyIcon.Visible = false;

                // Create context menu for the system tray icon
                var contextMenu = new ContextMenuStrip();

                var showMenuItem = new ToolStripMenuItem("Show Window", null, ShowApplication);
                var hideMenuItem = new ToolStripMenuItem("Hide Window", null, HideApplication);
                var separatorMenuItem = new ToolStripSeparator();
                var exitMenuItem = new ToolStripMenuItem("Exit Application", null, ExitApplication);

                contextMenu.Items.Add(showMenuItem);
                contextMenu.Items.Add(hideMenuItem);
                contextMenu.Items.Add(separatorMenuItem);
                contextMenu.Items.Add(exitMenuItem);

                _notifyIcon.ContextMenuStrip = contextMenu;

                // Double-click to show/hide the application
                _notifyIcon.DoubleClick += (s, e) => ToggleWindowVisibility();
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Failed to initialize system tray: {ex.Message}");
                _notifyIcon = null;
            }
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = true;
                    _notifyIcon.ShowBalloonTip(2000, "UDP System Monitor", "Application minimized to system tray", ToolTipIcon.Info);
                }
            }
        }

        private async void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            var settings = await _settingsService.LoadSettingsAsync();

            if (!_isClosing && settings.MinimizeToTrayOnClose)
            {
                e.Cancel = true;
                this.Hide();

                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = true;
                    _notifyIcon.ShowBalloonTip(2000, "UDP System Monitor", "Application minimized to system tray. Use context menu to exit.", ToolTipIcon.Info);
                }
            }
        }

        private void ToggleWindowVisibility()
        {
            if (this.IsVisible && this.WindowState != WindowState.Minimized)
            {
                HideApplication(null, EventArgs.Empty);
            }
            else
            {
                ShowApplication(null, EventArgs.Empty);
            }
        }

        private void ShowApplication(object? sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
            this.Topmost = true;  // Bring to front
            this.Topmost = false; // Remove topmost

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
            }
        }

        private void HideApplication(object? sender, EventArgs e)
        {
            this.Hide();

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = true;
            }
        }

        private void ExitApplication(object? sender, EventArgs e)
        {
            _isClosing = true;

            if (_notifyIcon != null)
            {
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            _viewModel.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from events to prevent memory leaks
            _viewModel.ActivityLogUpdated -= OnActivityLogUpdated;

            if (_notifyIcon != null)
            {
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            _viewModel?.Dispose();
            base.OnClosed(e);
        }
    }
}