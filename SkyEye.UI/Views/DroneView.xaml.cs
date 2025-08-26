using SkyEye.Connector;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SkyEye.UI.ViewModels;
using System.Windows.Threading;

namespace SkyEye.UI.Views
{
    /// <summary>
    /// Widok reprezentujący interfejs użytkownika do podglądu obrazu z drona.
    /// Odbiera strumień wideo z obiektu <see cref="IVideoReceiver"/> i wyświetla go w kontrolce.
    /// </summary>
    public partial class DroneView : Page
    {
        private byte[] _frame;
        private DispatcherTimer _videoTimer;
        private DroneViewModel _droneViewModel;
        private IVideoReceiver _videoReciver;

        /// <summary>
        /// Konstruktor widoku. Inicjalizuje model widoku, odbiornik wideo i uruchamia timer do renderowania obrazu.
        /// </summary>
        /// <param name="droneViewModel">Model widoku drona.</param>
        /// <param name="videoReciver">Odbiornik strumienia wideo.</param>
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
            _videoTimer.Interval = TimeSpan.FromSeconds(1 / 30);
            _videoTimer.Tick += RenderVideo;
            _videoTimer.Start();
        }

        /// <summary>
        /// Obsługuje zdarzenie odebrania nowej ramki wideo.
        /// </summary>
        /// <param name="data">Dane ramki obrazu w formacie bajtowym.</param>
        private void OnNewFrameRecived(byte[] data)
        {
            _frame = data;
        }

        /// <summary>
        /// Renderuje ramkę wideo w kontrolce interfejsu użytkownika.
        /// </summary>
        private void RenderVideo(object? sender, EventArgs e)
        {
            if (_frame == null)
                return;

            _mediaPlayer.Source = CreateBitmapFromRawData(_frame, 1280, 720, 3);
        }

        /// <summary>
        /// Tworzy obiekt <see cref="BitmapSource"/> na podstawie surowych danych pikseli.
        /// </summary>
        /// <param name="pixelData">Tablica bajtów zawierająca dane pikseli.</param>
        /// <param name="width">Szerokość obrazu.</param>
        /// <param name="height">Wysokość obrazu.</param>
        /// <param name="bytesPerPixel">Liczba bajtów przypadająca na jeden piksel.</param>
        /// <returns>Obiekt <see cref="BitmapSource"/> utworzony z danych wejściowych.</returns>
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
