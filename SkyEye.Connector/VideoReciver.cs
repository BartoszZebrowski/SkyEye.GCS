using Gst;
using Gst.App;
using OpenCvSharp;
using Task = System.Threading.Tasks.Task;

namespace SkyEye.Connector
{
    public class VideoReceiver : IVideoReceiver
    {
        private Pipeline _pipeline;
        private AppSink _appSink;

        public VideoReceiver() 
        {
        }

        private event Action<byte[]> _newFrameRecived;
        public event Action<byte[]> NewFrameRecived
        {
            add => _newFrameRecived += value;
            remove => _newFrameRecived -= value;
        }


        public async Task Init()
        {

            Application.Init();

            _pipeline = Parse.Launch("udpsrc port=9002 caps=\"application/x-rtp, payload=96\" ! rtph264depay ! h264parse ! avdec_h264 ! videoconvert ! video/x-raw, format=RGB ! appsink name=\"mysink\" max-buffers=10 drop=true") as Pipeline;


            var sink = _pipeline.GetByName("mysink");

            if (sink != null)
            {
                var sinkPad = sink.GetStaticPad("sink");

                sinkPad.AddProbe(PadProbeType.Buffer, (pad, info) =>
                {
                    using (var buffer = info.Buffer)
                    {
                        buffer.Map(out MapInfo mapInfo, MapFlags.Read);

                        int width = 1280;
                        int height = 720;
                        int channels = 3;


                        byte[] data = new byte[mapInfo.Data.Length];
                        byte[] image;


                        System.Array.Copy(mapInfo.Data, data, mapInfo.Data.Length);

                        //using (Mat mat = Mat.FromPixelData(height, width, MatType.CV_16SC4, mapInfo.Data))
                        //{
                        //    //CV_8UC3
                        //    if (mat == null || mat.Empty())
                        //    {
                        //        Console.WriteLine("Macierz obrazu jest pusta lub niezainicjalizowana!");
                        //    }
                        //    else
                        //    {
                        //        Cv2.ImShow("Stream", mat);
                        //    }

                        //    //Cv2.ImEncode(".jpg", mat, out image);
                        //}

                        

                        _newFrameRecived.Invoke(data);

                        Console.WriteLine($"Odebrano {mapInfo.Size} bajtów danych.");

                        buffer.Unmap(mapInfo);


                        return PadProbeReturn.Ok;
                    }


                });
            }
        }



        public void Play()
        {
            _pipeline?.SetState(State.Playing);
        }

        public void Pause()
        {
            _pipeline?.SetState(State.Paused);
        }

        public void Stop()
        {
            _pipeline?.SetState(State.Null);
        }
    }
}
