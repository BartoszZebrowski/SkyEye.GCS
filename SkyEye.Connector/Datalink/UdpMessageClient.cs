using SkyEye.Connector.Datalink;
using SkyEye.Connector.MessagesService;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Task = System.Threading.Tasks.Task;


namespace SkyEye.Connector.CommandService
{
    /// <summary>
    /// Klient UDP obsługujący wysyłanie i odbieranie wiadomości ze zdalnym serwerem.
    /// Odpowiada za synchronizację wartości przechowywanych w obiekcie Datalink.
    /// </summary>
    public class UdpMessageClient
    {
        private UdpClient? _client;
        private Datalink.Datalink _datalink;
        private IPEndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        private ConcurrentQueue<string> _remoteVariableToSetQueue = new();

        /// <summary>
        /// Konstruktor klienta UDP. Tworzy połączenie i przypisuje datalink.
        /// </summary>
        /// <param name="ip">Adres IP serwera.</param>
        /// <param name="port">Port serwera.</param>
        /// <param name="datalink">Obiekt Datalink przechowujący zdalne wartości.</param>
        public UdpMessageClient(string ip, int port, Datalink.Datalink datalink)
        {
            _datalink = datalink;
            Connect(ip, port);
        }

        /// <summary>
        /// Nawiązuje połączenie z serwerem UDP i uruchamia pętle wysyłania i odbierania.
        /// </summary>
        /// <param name="ip">Adres IP serwera.</param>
        /// <param name="port">Port serwera.</param>
        public void Connect(string ip, int port)
        {
            _client = new UdpClient();
            _client.Connect(ip, port);

            Task.Run(() => SendLoop());
            Task.Run(() => ReciveLoop());
        }

        /// <summary>
        /// Synchronizuje wszystkie zdalne wartości poprzez wysłanie żądań ich odczytu
        /// oraz ustawienie tych, które zostały oznaczone do aktualizacji.
        /// </summary>
        public void SynchroniseRemoteValues()
        {
            foreach (var remoteValue in _datalink.RemoteValues)
            {
                _remoteVariableToSetQueue.Enqueue(SerializeGetValueMessage(remoteValue));

                if (remoteValue.ToUpdate)
                {
                    _remoteVariableToSetQueue.Enqueue(SerializeSetValueMessage(remoteValue));
                    remoteValue.ToUpdate = false;
                }
            }
        }

        /// <summary>
        /// Pętla wysyłająca wiadomości do serwera.
        /// </summary>
        private void SendLoop()
        {
            while (true)
            {
                if (!_remoteVariableToSetQueue.TryDequeue(out string firstElement))
                    continue;

                _client.SendAsync(Encoding.UTF8.GetBytes(firstElement));
            }
        }

        /// <summary>
        /// Pętla odbierająca wiadomości od serwera.
        /// </summary>
        private void ReciveLoop()
        {
            while (true)
            {
                var recivedMessage = _client.Receive(ref _remoteEndPoint);
                var message = Encoding.UTF8.GetString(recivedMessage);
                DeserializeRemoteValue(message);
            }
        }

        /// <summary>
        /// Serializuje wiadomość ustawiającą wartość zdalną.
        /// </summary>
        /// <param name="remoteValue">Obiekt zdalnej wartości.</param>
        /// <returns>Łańcuch znaków reprezentujący wiadomość.</returns>
        private string SerializeSetValueMessage(IRemoteValue remoteValue)
        {
            string message = "";

            if (remoteValue is RemoteValue<int> intRemoteValue)
            {
                message = $"{(int)remoteValue.RemoteValueType};1;{intRemoteValue.GetValueToUpdate().ToString()}";
            }
            else if (remoteValue is RemoteValue<float> floatRemoteValue)
            {
                message = $"{(int)remoteValue.RemoteValueType};1;{Math.Round(floatRemoteValue.GetValueToUpdate(), 2).ToString().Replace(",", ".")}";
            }
            else if (remoteValue is RemoteValue<string> stringRemoteValue)
            {
                message = $"{(int)remoteValue.RemoteValueType};1;{stringRemoteValue.GetValueToUpdate().ToString()}";
            }

            return message;
        }

        /// <summary>
        /// Serializuje wiadomość żądającą odczytu wartości zdalnej.
        /// </summary>
        /// <param name="remoteValue">Obiekt zdalnej wartości.</param>
        /// <returns>Łańcuch znaków reprezentujący wiadomość.</returns>
        private string SerializeGetValueMessage(IRemoteValue remoteValue)
        {
            return $"{(int)remoteValue.RemoteValueType};0;0";
        }

        /// <summary>
        /// Deserializuje wiadomość otrzymaną od serwera i aktualizuje odpowiednią wartość.
        /// </summary>
        /// <param name="message">Odebrana wiadomość.</param>
        private void DeserializeRemoteValue(string message)
        {
            var splitedMessage = message.Split(";")
                ?? throw new Exception("Wrong data format");

            var remoteValueType = int.Parse(splitedMessage[0]);
            var stringValue = splitedMessage[1];

            var remoteValue = _datalink.RemoteValues.Where(rm => (int)rm.RemoteValueType == remoteValueType).First()
                ?? throw new Exception("This remote value don't exist");

            UpdateValue(remoteValue, stringValue);
        }

        /// <summary>
        /// Aktualizuje wartość w obiekcie RemoteValue na podstawie odebranych danych.
        /// </summary>
        /// <param name="remoteValue">Obiekt zdalnej wartości do aktualizacji.</param>
        /// <param name="value">Nowa wartość w formie tekstowej.</param>
        private void UpdateValue(IRemoteValue remoteValue, string value)
        {
            if (remoteValue is RemoteValue<int> intRemoteValue)
            {
                int commaIndex = value.IndexOf('.');

                if (commaIndex >= 0)
                {
                    value = value.Substring(0, commaIndex);
                }

                intRemoteValue.UpdateValue(int.Parse(value));
            }
            else if (remoteValue is RemoteValue<float> floatRemoteValue)
            {
                floatRemoteValue.UpdateValue(float.Parse(value.Replace(".", ",")));
            }
            else if (remoteValue is RemoteValue<string> stringRemoteValue)
            {
                stringRemoteValue.UpdateValue(value);
            }
            else if (remoteValue is RemoteValue<object> genericRemoteValue)
            {
                Type type = genericRemoteValue.Value.GetType();

                if (type.IsEnum)
                {
                    genericRemoteValue.UpdateValue(Enum.Parse(type, value));
                }
                else
                {
                    throw new InvalidOperationException($"Nieobsługiwany typ: {type}");
                }
            }
        }
    }

}
