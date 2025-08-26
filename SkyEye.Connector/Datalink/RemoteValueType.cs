using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.MessagesService
{
    /// <summary>
    /// Typy zdalnych wartości przesyłanych między urządzeniami.
    /// </summary>
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
