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
using System.Collections;
using System.IO;
using SkyEye.UI.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Threading;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using static OpenCvSharp.LineIterator;
using System.Windows.Ink;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Drawing.Rectangle;


namespace SkyEye.UI.Views
{
    /// <summary>
    /// Interaction logic for DroneView.xaml
    /// </summary>
    public partial class DroneView : Page
    {
        byte[] frame;
        private DispatcherTimer videoTimer;


        public DroneView(DroneViewModel droneViewModel)
        {
            DataContext = droneViewModel;

            InitializeComponent();

            _hud.SizeChanged += HudSizeChanged;


            IVideoReceiver videoReciver = new VideoReceiver();
            videoReciver.NewFrameRecived += OnNewFrameRecived;

            videoReciver.Init();
            videoReciver.Play();
            Console.WriteLine("Dupa");

            Console.WriteLine("Inicjalizacja");

            videoTimer = new DispatcherTimer();
            videoTimer.Interval = TimeSpan.FromSeconds(1/30);
            videoTimer.Tick += RenderVideo;
            videoTimer.Start();

        }

        private void HudSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double centerX = (_hud.ActualWidth - _gimbalPositionAreaEllipse.Width) / 2;
            double centerY = (_hud.ActualHeight - _gimbalPositionAreaEllipse.Height) / 2;
            _gimbalPositionAreaEllipseTransform.X = centerX;
            _gimbalPositionAreaEllipseTransform.Y = centerY;
        }

        private void OnNewFrameRecived(byte[] data)
        {

            frame = data;
        }

        private void RenderVideo(object? sender, EventArgs e)
        {

            if (frame == null)
                return;

            _mediaPlayer.Source = CreateBitmapFromRawData(frame, 1920, 1080, 3);

        }



        private BitmapSource CreateBitmapFromRawData(byte[] pixelData, int width, int height, int bytesPerPixel)
        {
            // Oblicz długość jednego wiersza (stride)
            int stride = width * bytesPerPixel;

            // Utwórz Bitmapę
            var bitmap = BitmapSource.Create(
                width,                          // Szerokość obrazu
                height,                         // Wysokość obrazu
                96,                             // DPI w poziomie
                96,                             // DPI w pionie
                bytesPerPixel == 3              // Format piksela (RGB lub RGBA)
                    ? System.Windows.Media.PixelFormats.Rgb24
                    : System.Windows.Media.PixelFormats.Bgra32,
                null,                           // Paleta (null dla obrazu TrueColor)
                pixelData,                      // Tablica bajtów z surowymi danymi
                stride                          // Długość jednego wiersza w bajtach
            );

            return bitmap;
        }
    }
}
