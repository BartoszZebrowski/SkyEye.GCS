using Gst;
using Task = System.Threading.Tasks.Task;

namespace SkyEye.Connector
{
    public class VideoReceiver : IVideoReceiver
    {

        public VideoReceiver() 
        {
        }

        public async Task RunVideo()
        {
            new Thread(() =>
            {

                // Initialize Gstreamer
                Application.Init();

                // Build the pipeline
                var pipeline = Parse.Launch("udpsrc port=9002 caps=\"application/x-rtp, payload=96\" ! rtph264depay ! h264parse ! avdec_h264 ! autovideosink");

                // Start playing
                pipeline.SetState(State.Playing);

                // Wait until error or EOS
                var bus = pipeline.Bus;
                var msg = bus.TimedPopFiltered(Constants.CLOCK_TIME_NONE, MessageType.Eos | MessageType.Error);

                // Free resources
                pipeline.SetState(State.Null);
            }).Start();
        }
    }
}
