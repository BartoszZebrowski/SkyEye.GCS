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

namespace SkyEye.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConfigureServices();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<TCPMessageClient>(provider => new TCPMessageClient("192.168.1.42", 5001));
            serviceCollection.AddSingleton<IncreesAngleControlCommand>();
            serviceCollection.AddSingleton<DroneView>();
            serviceCollection.AddSingleton<DroneViewModel>();
            serviceCollection.AddSingleton<Datalink>();
            serviceCollection.AddSingleton<MainWindow>();
            serviceCollection.AddSingleton<Timer>(provider => {
                var timer = new Timer(200);
                timer.AutoReset = true;
                timer.Start();
                return timer;
            });

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

    }
}
