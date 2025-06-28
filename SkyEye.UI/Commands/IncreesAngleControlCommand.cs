using SkyEye.Connector.Datalink;
using SkyEye.Connector.MessagesService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SkyEye.UI.Commands
{
    public class IncreesAngleControlCommand : ICommand
    {
        private static float _step = 1.0f;

        public event EventHandler? CanExecuteChanged;

        private RemoteValue<float> _horisonalAxisRemoteValue;
        private RemoteValue<float> _verticalAxisRemoteValue;

        public IncreesAngleControlCommand(Datalink datalink)
        {
            _horisonalAxisRemoteValue = datalink.GetRemoteValue<float>(RemoteValueType.TargetHorizontalAngle);
            _verticalAxisRemoteValue = datalink.GetRemoteValue<float>(RemoteValueType.TargetVerticalAngle);
        }

        public bool CanExecute(object? parameter)
            => true;

        public async void Execute(object? parameter)
        {
            switch ((string)parameter)
            {
                case "Up":
                    var angle1 = _verticalAxisRemoteValue.Value;
                     _verticalAxisRemoteValue.Value = (float)((angle1 + _step) / Math.PI);
                    break;
                case "Down":
                    var angle2 = _verticalAxisRemoteValue.Value;
                     _verticalAxisRemoteValue.Value = (float)((angle2 - _step) / Math.PI);
                    break;
                case "Right":
                    var angle3 = _horisonalAxisRemoteValue.Value;
                     _horisonalAxisRemoteValue.Value = (float)((angle3 + _step) / Math.PI); 
                    break;
                case "Left":
                    var angle4 = _horisonalAxisRemoteValue.Value;
                    _horisonalAxisRemoteValue.Value = (float)((angle4 - _step) / Math.PI);
                    break;
            }
        }
    }
}
