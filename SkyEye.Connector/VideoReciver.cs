using Gst;
using Gst.App;
using OpenCvSharp;
using System.Runtime.InteropServices;
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


        public async Task RunVideo()
        {
            Application.Init();

            _pipeline = Parse.Launch("udpsrc port=9002 caps=\"application/x-rtp, payload=96\" ! rtph264depay ! h264parse ! avdec_h264 ! videoconvert ! decodebin ! appsink name=\"mysink\"") as Pipeline;


            var sink = _pipeline.GetByName("mysink");

            if (sink != null)
            {
                var sinkPad = sink.GetStaticPad("sink");

                sinkPad.AddProbe(PadProbeType.Buffer, (pad, info) =>
                {
                    var buffer = info.Buffer;

                    buffer.Map(out MapInfo mapInfo, MapFlags.Read);

                    int width = 640;
                    int height = 480;
                    int channels = 3;

                    byte[] data = new byte[mapInfo.Data.Length];

                    System.Array.Copy(mapInfo.Data, data, mapInfo.Data.Length);

                    using (Mat mat = Mat.FromPixelData(height, width, MatType.CV_8UC3, data))
                    {

                        try
                        {
                            Cv2.ImShow("Stream", mat);
                            Cv2.WaitKey(1);

                        }
                        catch (Exception ex)
                        {

                        }

                    }




                    //_newFrameRecived.Invoke(mapInfo.Data);
                    Console.WriteLine($"Odebrano {mapInfo.Size} bajtów danych.");

                    buffer.Unmap(mapInfo);


                    return PadProbeReturn.Ok;
                });
            }

            Play();

        }

        //private void OnNewSample(object sender, NewSampleArgs args)
        //{
        //    var sample = _appSink.PullSample();

        //    
        //    var buffer = sample.Buffer;
        //    var mapInfo = new Gst.MapInfo();
        //    buffer.Map(out mapInfo, Gst.MapFlags.Read);

        //    try
        //    {
        //        Console.WriteLine($"Pobrano klatkę: {mapInfo.Size} bajtów");
        //        _newFrameRecived.Invoke(mapInfo.Data);
        //    }
        //    finally
        //    {
        //        buffer.Unmap(mapInfo);
        //        sample.Dispose();
        //    }
        //}

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
