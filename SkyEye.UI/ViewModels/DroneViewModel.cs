using Gst.Video;
using SkyEye.Connector;
using SkyEye.Connector.Datalink;
using SkyEye.Connector.MessagesService;
using SkyEye.UI.Commands;
using SkyEye.UI.Common;
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
    public class DroneViewModel : NotifyPropertyChanged
    {
        //do wywalenia pewnie całosc
        public BitmapImage VideoFrame { get; private set; }

        public IncreesAngleControlCommand IncreesAngleControlCommand { get; private set; }
        public int HorisonalAxis { get; private set; }

        private Datalink _datalink;


        public DroneViewModel(Datalink datalink, IncreesAngleControlCommand increesAngleControlCommand)
        {
            _datalink = datalink;

            IVideoReceiver videoReciver = new VideoReceiver();
            videoReciver.NewFrameRecived += OnNewFrameRecived;

            videoReciver.Init();
            videoReciver.Play();


            IncreesAngleControlCommand = increesAngleControlCommand;
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
