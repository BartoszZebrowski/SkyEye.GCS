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
        public ChangeAngleControlCommand IncreesAngleControlCommand { get; private set; }
        public ChangeModeCommand ChangeModeCommand { get; private set; }
        public ChangeZoomCommand ChangeZoomCommand { get; private set; }

        private float _horisonalAxis = 37;
        public float HorisonalAxis 
        {
            get => _horisonalAxis;
            private set => SetField(ref _horisonalAxis, value); 
        }

        private float _verticalAxis = 3;
        public float VerticalAxis 
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
            ChangeAngleControlCommand increesAngleControlCommand, 
            ChangeModeCommand changeModeCommand,
            ChangeZoomCommand changeZoomCommand)
        {
            _datalink = datalink;

            IncreesAngleControlCommand = increesAngleControlCommand;
            ChangeModeCommand = changeModeCommand;
            ChangeZoomCommand = changeZoomCommand;

            //_datalink.GetRemoteValue<float>(RemoteValueType.TargetVerticalAngle).ValueChanged 
            //    += value => VerticalAxis = value;

            //_datalink.GetRemoteValue<float>(RemoteValueType.TargetHorizontalAngle).ValueChanged
            //    += value => HorisonalAxis = value;

            //_datalink.GetRemoteValue<float>(RemoteValueType.ZoomValue).ValueChanged
            //    += value => ZoomValue = value;

            OnPropertyChanged();
        }
    }
}
