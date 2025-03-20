using SkyEye.Connector.Datalink;
using SkyEye.Connector.MessagesService;
using SkyEye.UI.Commands;
using SkyEye.UI.Common;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using Timer = System.Timers.Timer;

namespace SkyEye.UI.ViewModels
{
    public class DroneViewModel : NotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public BitmapImage VideoFrame { get; private set; }
        public IncreesAngleControlCommand IncreesAngleControlCommand { get; private set; }
        public ChangeModeCommand ChangeModeCommand { get; private set; }
        public ChangeZoomCommand ChangeZoomCommand { get; private set; }

        private int _horisonalAxis;
        public int HorisonalAxis 
        {
            get => _horisonalAxis;
            private set => SetField(ref _horisonalAxis, value); 
        }

        private int _verticalAxis;
        public int VerticalAxis 
        {
            get => _verticalAxis;
            private set => SetField(ref _verticalAxis, value);
        }

        private float _zoomValue;
        public float ZoomValue
        {
            get => _zoomValue;
            private set => SetField(ref _zoomValue, value);
        }

        private Datalink _datalink;

        public DroneViewModel(Datalink datalink, 
            IncreesAngleControlCommand increesAngleControlCommand, 
            ChangeModeCommand changeModeCommand,
            ChangeZoomCommand changeZoomCommand)
        {
            _datalink = datalink;

            IncreesAngleControlCommand = increesAngleControlCommand;
            ChangeModeCommand = changeModeCommand;
            ChangeZoomCommand = changeZoomCommand;

            _datalink.GetRemoteValue<int>(RemoteValueType.TargetVerticalAngle).ValueChanged 
                += value => VerticalAxis = value;

            _datalink.GetRemoteValue<int>(RemoteValueType.TargetHorizontalAngle).ValueChanged
                += value => HorisonalAxis = value;

            _datalink.GetRemoteValue<float>(RemoteValueType.ZoomValue).ValueChanged
                += value => ZoomValue = value;

            OnPropertyChanged();
        }
    }
}
