using Gst.Video;
using SkyEye.Connector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SkyEye.UI.ViewModels
{
    internal class DroneViewModel : INotifyPropertyChanged
    {
        //do wywalenia pewnie całosc
        public BitmapImage VideoFrame { get; set; }

        public DroneViewModel()
        {
            IVideoReceiver videoReciver = new VideoReceiver();
            videoReciver.NewFrameRecived += OnNewFrameRecived;

            videoReciver.Init();
            videoReciver.Play();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnNewFrameRecived(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                // Tworzymy BitmapImage z MemoryStream
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.EndInit();

                // Przypisujemy BitmapImage do Image w WPF
                VideoFrame = bitmap;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("VideoFrame"));
            }
        }
    }
}
