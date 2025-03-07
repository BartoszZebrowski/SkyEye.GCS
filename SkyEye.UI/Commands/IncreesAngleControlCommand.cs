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
            _horisonalAxisRemoteValue = datalink.GetRemoteValue<int>(RemoteValueType.HorisonalAxis);
            _verticalAxisRemoteValue = datalink.GetRemoteValue<int>(RemoteValueType.VerticalAxis);
        }

        public bool CanExecute(object? parameter)
            => true;

        public async void Execute(object? parameter)
        {
            switch ((string)parameter)
            {
                case "Up":
                    var angle1 = await _verticalAxisRemoteValue.Get();
                    await _verticalAxisRemoteValue.Set(angle1 + 1);
                    break;
                case "Down":
                    var angle2 = await _verticalAxisRemoteValue.Get();
                    await _verticalAxisRemoteValue.Set(angle2 - 1);
                    break;
                case "Right":
                    var angle3 = await _verticalAxisRemoteValue.Get();
                    await _horisonalAxisRemoteValue.Set(angle3 + 1);
                    break;
                case "Left":
                    var angle4 = await _verticalAxisRemoteValue.Get();
                    await _horisonalAxisRemoteValue.Set(angle4 - 1);
                    break;
            }
        }
    }
}
