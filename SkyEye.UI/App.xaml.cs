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

        DispatcherTimer _dispatcherTimer = new DispatcherTimer();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConfigureServices();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            var TCPMessageClient = ServiceProvider.GetRequiredService<WebsocketClient>();

            _dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _dispatcherTimer.Tick += (sender, e) =>
            {
                TCPMessageClient.UpdateRemoteValues();
            };

            _dispatcherTimer.Start();
        }

        private void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<Datalink>();

            serviceCollection.AddSingleton<WebsocketClient>(provider => 
            new WebsocketClient(
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
