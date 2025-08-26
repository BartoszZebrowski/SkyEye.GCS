using SkyEye.Connector.MessagesService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector.Datalink
{
    /// <summary>
    /// Interfejs bazowy dla wszystkich zdalnych wartości.
    /// Definiuje wspólne właściwości, takie jak typ wartości oraz flaga aktualizacji.
    /// </summary>
    public interface IRemoteValue
    {
        /// <summary>
        /// Typ zdalnej wartości.
        /// </summary>
        RemoteValueType RemoteValueType { get; init; }

        /// <summary>
        /// Flaga informująca, czy wartość powinna zostać zaktualizowana.
        /// </summary>
        public bool ToUpdate { get; set; }
    }
}
