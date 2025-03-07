using SkyEye.Connector.CommandService;
using SkyEye.Connector.MessagesService;
using SkyEye.UI.Views;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SkyEye.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        public MainWindow(DroneView droneView)
        {
            InitializeComponent();
            AllocConsole();

            MainFrame.Navigate(droneView);

            StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput())
            {
                AutoFlush = true
            };
            Console.SetOut(standardOutput);

            Console.WriteLine("Consol work!");
        }
    }
}