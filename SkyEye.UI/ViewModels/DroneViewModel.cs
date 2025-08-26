using SkyEye.Connector.Datalink;
using SkyEye.Connector.MessagesService;
using SkyEye.UI.Commands;
using SkyEye.UI.Common;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using Timer = System.Timers.Timer;

namespace SkyEye.UI.ViewModels
{
    /// <summary>
    /// Model widoku (ViewModel) reprezentujący drona.
    /// Udostępnia właściwości powiązane z kątami sterowania i zoomem,
    /// a także komendy do zmiany trybu, kąta i przybliżenia.
    /// </summary>
    public class DroneViewModel : NotifyPropertyChanged
    {
        /// <summary>
        /// Zdarzenie informujące o zmianie właściwości.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Aktualna klatka wideo w postaci obrazu.
        /// </summary>
        public BitmapImage VideoFrame { get; private set; }

        /// <summary>
        /// Komenda zmiany kąta sterowania.
        /// </summary>
        public ChangeAngleControlCommand IncreesAngleControlCommand { get; private set; }

        /// <summary>
        /// Komenda zmiany trybu pracy.
        /// </summary>
        public ChangeModeCommand ChangeModeCommand { get; private set; }

        /// <summary>
        /// Komenda zmiany zoomu.
        /// </summary>
        public ChangeZoomCommand ChangeZoomCommand { get; private set; }

        private float _horisonalAxis;

        /// <summary>
        /// Aktualny kąt poziomy.
        /// </summary>
        public float HorisonalAxis
        {
            get => _horisonalAxis;
            private set => SetField(ref _horisonalAxis, value);
        }

        private float _verticalAxis;

        /// <summary>
        /// Aktualny kąt pionowy.
        /// </summary>
        public float VerticalAxis
        {
            get => _verticalAxis;
            private set => SetField(ref _verticalAxis, value);
        }

        private float _zoomValue;

        /// <summary>
        /// Aktualna wartość zoomu.
        /// </summary>
        public float ZoomValue
        {
            get => _zoomValue;
            private set => SetField(ref _zoomValue, value);
        }

        private Datalink _datalink;

        /// <summary>
        /// Konstruktor modelu widoku drona.
        /// Subskrybuje zmiany wartości zdalnych i aktualizuje powiązane właściwości.
        /// </summary>
        /// <param name="datalink">Obiekt Datalink z dostępem do zdalnych wartości.</param>
        /// <param name="increesAngleControlCommand">Komenda do zmiany kąta sterowania.</param>
        /// <param name="changeModeCommand">Komenda do zmiany trybu pracy.</param>
        /// <param name="changeZoomCommand">Komenda do zmiany zoomu.</param>
        public DroneViewModel(Datalink datalink,
            ChangeAngleControlCommand increesAngleControlCommand,
            ChangeModeCommand changeModeCommand,
            ChangeZoomCommand changeZoomCommand)
        {
            _datalink = datalink;

            IncreesAngleControlCommand = increesAngleControlCommand;
            ChangeModeCommand = changeModeCommand;
            ChangeZoomCommand = changeZoomCommand;

            _datalink.GetRemoteValue<float>(RemoteValueType.TargetVerticalAngle).ValueChanged
                += value => VerticalAxis = value;

            _datalink.GetRemoteValue<float>(RemoteValueType.TargetHorizontalAngle).ValueChanged
                += value => HorisonalAxis = value;

            _datalink.GetRemoteValue<float>(RemoteValueType.ZoomValue).ValueChanged
                += value => ZoomValue = value;

            OnPropertyChanged();
        }
    }
}
