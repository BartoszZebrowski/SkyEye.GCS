namespace SkyEye.Connector
{
    public interface IVideoReceiver
    {
        Task Init();
        event Action<byte[]> NewFrameRecived;
        void Play();
        void Pause();
        void Stop();
    }
}
