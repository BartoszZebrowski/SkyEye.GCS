namespace SkyEye.Connector
{
    /// <summary>
    /// Interfejs odbiornika strumienia wideo.
    /// Definiuje podstawowe operacje związane z inicjalizacją i sterowaniem odtwarzaniem.
    /// </summary>
    public interface IVideoReceiver
    {
        /// <summary>
        /// Inicjalizuje odbiornik wideo.
        /// </summary>
        Task Init();

        /// <summary>
        /// Zdarzenie wywoływane po odebraniu nowej ramki wideo.
        /// </summary>
        event Action<byte[]> NewFrameRecived;

        /// <summary>
        /// Rozpoczyna odtwarzanie strumienia wideo.
        /// </summary>
        void Play();

        /// <summary>
        /// Wstrzymuje odtwarzanie strumienia wideo.
        /// </summary>
        void Pause();

        /// <summary>
        /// Zatrzymuje odbiór strumienia wideo.
        /// </summary>
        void Stop();
    }
}
