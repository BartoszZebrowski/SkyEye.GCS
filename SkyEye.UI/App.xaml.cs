using Microsoft.Extensions.DependencyInjection;
using SkyEye.Connector.CommandService;
using SkyEye.Connector.Datalink;
using SkyEye.UI.ViewModels;
using SkyEye.UI.Views;
using System.Timers;
using System.Configuration;
using System.Data;
using System.Windows;
using Timer = System.Timers.Timer;
using SkyEye.UI.Commands;
using System.Windows.Threading;

namespace SkyEye.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        DispatcherTimer _updateRemoteValuesTimer = new DispatcherTimer();

        UdpMessageClient _TCPMessageClient;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConfigureServices();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            _TCPMessageClient = ServiceProvider.GetRequiredService<UdpMessageClient>();

            _updateRemoteValuesTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            _updateRemoteValuesTimer.Tick += (sender, e) =>
            {
                _TCPMessageClient.SynchroniseRemoteValues();
            };

            _updateRemoteValuesTimer.Start();
        }

        private void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<Datalink>();

            serviceCollection.AddSingleton<UdpMessageClient>(provider => 
            new UdpMessageClient(
                "192.168.1.42",
                5001,
                ServiceProvider.GetRequiredService<Datalink>()));

            serviceCollection.AddSingleton<IncreesAngleControlCommand>();
            serviceCollection.AddSingleton<ChangeModeCommand>();
            serviceCollection.AddSingleton<DroneView>();
            serviceCollection.AddSingleton<DroneViewModel>();
            serviceCollection.AddSingleton<MainWindow>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
