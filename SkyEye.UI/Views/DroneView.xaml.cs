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


namespace SkyEye.UI.Views
{
    /// <summary>
    /// Interaction logic for DroneView.xaml
    /// </summary>
    public partial class DroneView : Page
    {
        List<byte[]> frameBuffor = new();

        private DispatcherTimer videoTimer;


        public DroneView()
        {
            InitializeComponent();

            //var droneViewModel = new DroneViewModel();
            //droneViewModel.PropertyChanged += Update;


            //DataContext = droneViewModel;

            IVideoReceiver videoReciver = new VideoReceiver();
            videoReciver.NewFrameRecived += OnNewFrameRecived;

            videoReciver.Init();
            videoReciver.Play();
            Console.WriteLine("Dupa");

            Console.WriteLine("Inicjalizacja");

            videoTimer = new DispatcherTimer();
            videoTimer.Interval = TimeSpan.FromSeconds(1/60);
            videoTimer.Tick += RenderVideo;
            videoTimer.Start();

        }

        private void OnNewFrameRecived(byte[] data)
        {
            lock (frameBuffor)
            {
                if(frameBuffor.Count > 10)
                {
                    frameBuffor.Remove(frameBuffor.First());
                    frameBuffor.Add(data);
                }
                else
                {
                    frameBuffor.Add(data);
                }
            }
        }

        private void RenderVideo(object? sender, EventArgs e)
        {
            if (frameBuffor.Count == 0)
                return;

            var frame = frameBuffor.First();

            using (MemoryStream stream = new MemoryStream(frame))
            {
                // Tworzymy BitmapImage z MemoryStream
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();


                //var x = new WriteableBitmap(bitmap);

                //var bytePerPixel = 3;
                //var stride = bytePerPixel * 320;

                //x.WritePixels(new Int32Rect(0, 0, 320, 240), frame, stride, 0);

                _mediaPlayer.Source = bitmap;
                _mediaPlayer.UpdateLayout();
            }

            //_testButton.Content = frameBuffor.Count.ToString();

            UpdateLayout();

            frameBuffor.Remove(frame);

            Console.WriteLine("Klatka");
            Console.WriteLine("Wielkosc Bufora: " + frameBuffor.Count);

        }
    }
}
