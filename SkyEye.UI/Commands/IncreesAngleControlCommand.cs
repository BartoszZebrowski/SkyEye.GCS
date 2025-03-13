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
        public event EventHandler? CanExecuteChanged;

        private RemoteValue<int> _horisonalAxisRemoteValue;
        private RemoteValue<int> _verticalAxisRemoteValue;

        public IncreesAngleControlCommand(Datalink datalink)
        {
            _horisonalAxisRemoteValue = datalink.GetRemoteValue<int>(RemoteValueType.TargetHorizontalAngle);
            _verticalAxisRemoteValue = datalink.GetRemoteValue<int>(RemoteValueType.TargetVerticalAngle);
        }

        public bool CanExecute(object? parameter)
            => true;

        public async void Execute(object? parameter)
        {
            switch ((string)parameter)
            {
                case "Up":
                    var angle1 = _verticalAxisRemoteValue.Value;
                     _verticalAxisRemoteValue.Value = angle1 + 1;
                    break;
                case "Down":
                    var angle2 = _verticalAxisRemoteValue.Value;
                     _verticalAxisRemoteValue.Value = angle2 - 1;
                    break;
                case "Right":
                    var angle3 = _horisonalAxisRemoteValue.Value;
                     _horisonalAxisRemoteValue.Value = angle3 + 1; 
                    break;
                case "Left":
                    var angle4 = _horisonalAxisRemoteValue.Value;
                    _horisonalAxisRemoteValue.Value = angle4 - 1;
                    break;
            }
        }
    }
}
