using SkyEye.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SkyEye.Connector;


namespace SkyEye.UI.Views
{
    /// <summary>
    /// Interaction logic for DroneView.xaml
    /// </summary>
    public partial class DroneView : Page
    {
        public DroneView()
        {
            InitializeComponent();

            IVideoReceiver videoReciver = new VideoReceiver();
            videoReciver.NewFrameRecived += OnNewFrameRecived;

            videoReciver.RunVideo();
            Console.WriteLine("Inicjalizacja");

        }

        private void OnNewFrameRecived(byte[] data)
        {
            Console.WriteLine("Test");
        }
    }
}
