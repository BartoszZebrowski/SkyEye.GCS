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
        //do wywalenia pewnie całosc
        public BitmapImage VideoFrame { get; private set; }
        public IncreesAngleControlCommand IncreesAngleControlCommand { get; private set; }
        public ChangeModeCommand ChangeModeCommand { get; private set; }

        private int _horisonalAxis;
        public int HorisonalAxis 
        {
            get => _horisonalAxis;
            private set => SetField(ref _horisonalAxis, value); 
        }

        private int _verticalAxis;
        public int VerticalAxis {
            get => _verticalAxis;
            private set 
            {
                _verticalAxis = value;
                OnPropertyChanged(nameof(VerticalAxis));
            }
        }

        private Datalink _datalink;

        public DroneViewModel(Datalink datalink, IncreesAngleControlCommand increesAngleControlCommand, ChangeModeCommand changeModeCommand)
        {
            _datalink = datalink;

            IncreesAngleControlCommand = increesAngleControlCommand;
            ChangeModeCommand = changeModeCommand;

            _datalink.GetRemoteValue<int>(RemoteValueType.TargetVerticalAngle).ValueChanged 
                += value => VerticalAxis = value;

            _datalink.GetRemoteValue<int>(RemoteValueType.TargetHorizontalAngle).ValueChanged
                += value => HorisonalAxis = value;

            OnPropertyChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
