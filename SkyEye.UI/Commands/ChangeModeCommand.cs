using SkyEye.Connector.Datalink;
using SkyEye.Connector.MessagesService;
using SkyEye.Connector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SkyEye.UI.Commands
{
    public class ChangeModeCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private RemoteValue<WorkingMode> _workingModeRemoteValue;

        public ChangeModeCommand(Datalink datalink)
        {
            _workingModeRemoteValue = datalink.GetRemoteValue<WorkingMode>(RemoteValueType.WorkingMode);
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public async void Execute(object? parameter)
        {
            switch ((string)parameter)
            {
                case "ManualMode":
                    _workingModeRemoteValue.Value = WorkingMode.ManualMode;
                    break;
                case "FollowMode":
                    _workingModeRemoteValue.Value = WorkingMode.FollowMode;
                    break;
            }
        }
    }
}
