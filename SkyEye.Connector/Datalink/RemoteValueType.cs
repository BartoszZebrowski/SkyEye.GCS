using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.MessagesService
{
    public enum RemoteValueType
    {
        Ping = 0,
        WorkingMode,
        TargetHorizontalAngle,
        TargetVerticalAngle,
        ActualHorizontaAngle,
        ActualVerticalAngle,
        ZoomValue,
    }
}
