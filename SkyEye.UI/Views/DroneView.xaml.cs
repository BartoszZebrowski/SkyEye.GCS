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
        List<byte[]> frameBuffor = new();

        byte[] frame;

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
            videoTimer.Interval = TimeSpan.FromSeconds(1/30);
            videoTimer.Tick += RenderVideo;
            videoTimer.Start();

        }

        private void OnNewFrameRecived(byte[] data)
        {
            //lock (frameBuffor)
            //{
            //    if(frameBuffor.Count > 10)
            //    {
            //        frameBuffor.Remove(frameBuffor.First());
            //        frameBuffor.Add(data);
            //    }
            //    else
            //    {
            //        frameBuffor.Add(data);
            //    }
            //}

            frame = data;
        }

        private void RenderVideo(object? sender, EventArgs e)
        {
            //if (frameBuffor.Count == 0)
            //    return;

            //var frame = frameBuffor.First();

            if (frame == null)
                return;

            _mediaPlayer.Source = CreateBitmapFromRawData(frame, 1920, 1080, 3);

            //Task.Run(() =>
            //{
            //}).Start();
        }

        private void RenderVideo(byte[] frame)
        {
            _mediaPlayer.Source = CreateBitmapFromRawData(frame, 1920, 1080, 3);

            _mediaPlayer.UpdateLayout();
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

        public byte[] ConvertNV12ToRGB(byte[] nv12Data, int width, int height)
        {
            int frameSize = width * height * 3; // 3 bajty na piksel w RGB
            byte[] rgbData = new byte[frameSize];

            int yIndex = 0;
            int uvIndex = width * height; // UV zaczyna się po Y
            int rgbIndex = 0;

            for (int i = 0; i < width * height; i++)
            {
                byte y = nv12Data[yIndex++];
                byte u = nv12Data[uvIndex++];
                byte v = nv12Data[uvIndex++];

                // Konwersja z YUV do RGB
                int r = (int)(y + 1.402 * (v - 128));
                int g = (int)(y - 0.344136 * (u - 128) - 0.714136 * (v - 128));
                int b = (int)(y + 1.772 * (u - 128));

                // Ogranicz wartości do zakresu 0-255
                r = Math.Min(255, Math.Max(0, r));
                g = Math.Min(255, Math.Max(0, g));
                b = Math.Min(255, Math.Max(0, b));

                // Zapisz wynikowy kolor w formacie RGB
                rgbData[rgbIndex++] = (byte)r;
                rgbData[rgbIndex++] = (byte)g;
                rgbData[rgbIndex++] = (byte)b;
            }

            return rgbData;
        }

        public static Bitmap TransformNv12ToBmpFaster(byte[] data, int width, int height)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppRgb);

            var uvStart = width * height;
            for (var y = 0; y < height; y++)
            {
                var pos = y * width;
                var posInBmp = y * bmpData.Stride;
                for (var x = 0; x < width; x++)
                {
                    var vIndex = uvStart + ((y >> 1) * width) + (x & ~1);

                    //// https://msdn.microsoft.com/en-us/library/windows/desktop/dd206750(v=vs.85).aspx
                    //// https://en.wikipedia.org/wiki/YUV
                    var c = data[pos] - 16;
                    var d = data[vIndex] - 128;
                    var e = data[vIndex + 1] - 128;
                    c = c < 0 ? 0 : c;

                    var r = ((298 * c) + (409 * e) + 128) >> 8;
                    var g = ((298 * c) - (100 * d) - (208 * e) + 128) >> 8;
                    var b = ((298 * c) + (516 * d) + 128) >> 8;
                    r = int.Clamp(r, 0, 255);
                    g = int.Clamp(g, 0, 255);
                    b = int.Clamp(b, 0, 255);

                    Marshal.WriteInt32(bmpData.Scan0, posInBmp + (x << 2), (b << 0) | (g << 8) | (r << 16) | (0xFF << 24));
                    pos++;
                }
            }

            bmp.UnlockBits(bmpData);

            watch.Stop();

            return bmp;
        }
    }
}
