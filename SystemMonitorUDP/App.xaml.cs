using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;
using SystemMonitorUDP.Services;
using SystemMonitorUDP.ViewModels;

namespace SystemMonitorUDP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private IHost? _host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Call base first
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ISystemMonitorService, SystemMonitorService>();
                    services.AddSingleton<IUdpService, UdpService>();
                    services.AddSingleton<ISettingsService, SettingsService>();
                    services.AddSingleton<IStartupService, StartupService>();
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            await _host.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            base.OnExit(e);
        }
    }
}
