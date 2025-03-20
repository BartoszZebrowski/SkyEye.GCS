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
    public class ChangeZoomCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private RemoteValue<float> _zoomValueRemoteValue; 

        public ChangeZoomCommand(Datalink datalink)
        {
            _zoomValueRemoteValue = datalink.GetRemoteValue<float>(RemoteValueType.ZoomValue);
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is null)
                return;

            var zoomValue = float.Parse((string)parameter);

            _zoomValueRemoteValue.Value += zoomValue;
        }
    }
}
