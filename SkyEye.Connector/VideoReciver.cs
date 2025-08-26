using Gst;
using Gst.App;
using OpenCvSharp;
using Task = System.Threading.Tasks.Task;

namespace SkyEye.Connector
{
    /// <summary>
    /// Odbiornik strumienia wideo realizowany przy użyciu GStreamer.
    /// Implementuje interfejs <see cref="IVideoReceiver"/> umożliwiający inicjalizację
    /// oraz sterowanie odtwarzaniem.
    /// </summary>
    public class VideoReceiver : IVideoReceiver
    {
        private Pipeline _pipeline;
        private AppSink _appSink;

        /// <summary>
        /// Konstruktor odbiornika wideo.
        /// </summary>
        public VideoReceiver()
        {
        }

        private event Action<byte[]> _newFrameRecived;

        /// <summary>
        /// Zdarzenie wywoływane po odebraniu nowej ramki wideo.
        /// </summary>
        public event Action<byte[]> NewFrameRecived
        {
            add => _newFrameRecived += value;
            remove => _newFrameRecived -= value;
        }

        /// <summary>
        /// Inicjalizuje potok GStreamer do odbioru strumienia wideo przez UDP.
        /// Konfiguruje <c>appsink</c> i rejestruje procedurę obsługi dla nowych buforów.
        /// </summary>
        public async Task Init()
        {
            Application.Init();
            _pipeline = Parse.Launch("udpsrc port=9002 caps=\"application/x-rtp, payload=96\" ! rtph264depay ! h264parse ! openh264dec ! videoconvert ! video/x-raw, format=RGB ! appsink name=\"mysink\" max-buffers=50 drop=true") as Pipeline;
            var sink = _pipeline.GetByName("mysink");

            if (sink == null)
                return;

            var sinkPad = sink.GetStaticPad("sink");

            sinkPad.AddProbe(PadProbeType.Buffer, (pad, info) =>
            {
                using (var buffer = info.Buffer)
                {
                    buffer.Map(out MapInfo mapInfo, MapFlags.Read);

                    byte[] data = new byte[mapInfo.Data.Length];

                    System.Array.Copy(mapInfo.Data, data, mapInfo.Data.Length);

                    _newFrameRecived?.Invoke(data);

                    buffer.Unmap(mapInfo);
                    return PadProbeReturn.Ok;
                }
            });
        }

        /// <summary>
        /// Uruchamia odtwarzanie strumienia wideo.
        /// </summary>
        public void Play()
        {
            _pipeline?.SetState(State.Playing);
        }

        /// <summary>
        /// Wstrzymuje odtwarzanie strumienia wideo.
        /// </summary>
        public void Pause()
        {
            _pipeline?.SetState(State.Paused);
        }

        /// <summary>
        /// Zatrzymuje i resetuje potok GStreamer.
        /// </summary>
        public void Stop()
        {
            _pipeline?.SetState(State.Null);
        }
    }

}
