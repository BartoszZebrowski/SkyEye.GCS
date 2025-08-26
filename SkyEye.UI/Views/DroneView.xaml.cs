using SkyEye.Connector;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SkyEye.UI.ViewModels;
using System.Windows.Threading;




namespace SkyEye.UI.Views
{
    /// <summary>
    /// Interaction logic for DroneView.xaml
    /// </summary>
    public partial class DroneView : Page
    {
        private byte[] _frame;
        private DispatcherTimer _videoTimer;
        private DroneViewModel _droneViewModel;
        private IVideoReceiver _videoReciver;

        public DroneView(DroneViewModel droneViewModel, IVideoReceiver videoReciver)
        {
            InitializeComponent();
            DataContext = droneViewModel;

            _droneViewModel = droneViewModel;

            _videoReciver = videoReciver;
            _videoReciver.NewFrameRecived += OnNewFrameRecived;

            videoReciver.Init();
            videoReciver.Play();

            _videoTimer = new DispatcherTimer();
            _videoTimer.Interval = TimeSpan.FromSeconds(1/30);
            _videoTimer.Tick += RenderVideo;
            _videoTimer.Start();

        }


        private void OnNewFrameRecived(byte[] data)
        {
            _frame = data;
        }

        private void RenderVideo(object? sender, EventArgs e)
        {
            if (_frame == null)
                return;

            _mediaPlayer.Source = CreateBitmapFromRawData(_frame, 1280, 720, 3);
        }

        private BitmapSource CreateBitmapFromRawData(byte[] pixelData, int width, int height, int bytesPerPixel)
        {
            int stride = width * bytesPerPixel;

            var bitmap = BitmapSource.Create(width, height, 96, 96, 
                bytesPerPixel == 3
                    ? System.Windows.Media.PixelFormats.Rgb24
                    : System.Windows.Media.PixelFormats.Bgra32,
                null,
                pixelData,
                stride
            );

            return bitmap;
        }
    }
}
