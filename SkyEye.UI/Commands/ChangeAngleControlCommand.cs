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
    /// <summary>
    /// Komenda zmieniająca kąt sterowania w poziomie i pionie.
    /// Implementuje interfejs <see cref="ICommand"/> do obsługi zdarzeń w interfejsie użytkownika.
    /// </summary>
    public class ChangeAngleControlCommand : ICommand
    {
        private static float _step = 0.1f;

        /// <summary>
        /// Zdarzenie sygnalizujące zmianę stanu możliwości wykonania komendy.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        private RemoteValue<float> _horisonalAxisRemoteValue;
        private RemoteValue<float> _verticalAxisRemoteValue;

        /// <summary>
        /// Konstruktor komendy. Inicjalizuje dostęp do wartości zdalnych osi.
        /// </summary>
        /// <param name="datalink">Obiekt datalink z dostępem do zdalnych wartości.</param>
        public ChangeAngleControlCommand(Datalink datalink)
        {
            _horisonalAxisRemoteValue = datalink.GetRemoteValue<float>(RemoteValueType.TargetHorizontalAngle);
            _verticalAxisRemoteValue = datalink.GetRemoteValue<float>(RemoteValueType.TargetVerticalAngle);
        }

        /// <summary>
        /// Określa, czy komenda może być wykonana.
        /// </summary>
        /// <param name="parameter">Parametr komendy.</param>
        /// <returns>Zawsze zwraca true.</returns>
        public bool CanExecute(object? parameter)
            => true;

        /// <summary>
        /// Wykonuje komendę zmiany kąta w zależności od podanego kierunku.
        /// </summary>
        /// <param name="parameter">Kierunek zmiany kąta: "Up", "Down", "Right", "Left".</param>
        public async void Execute(object? parameter)
        {
            switch ((string)parameter)
            {
                case "Up":
                    var angle1 = _verticalAxisRemoteValue.Value;
                    _verticalAxisRemoteValue.Value = angle1 + _step;
                    break;
                case "Down":
                    var angle2 = _verticalAxisRemoteValue.Value;
                    _verticalAxisRemoteValue.Value = angle2 - _step;
                    break;
                case "Right":
                    var angle3 = _horisonalAxisRemoteValue.Value;
                    _horisonalAxisRemoteValue.Value = angle3 + _step;
                    break;
                case "Left":
                    var angle4 = _horisonalAxisRemoteValue.Value;
                    _horisonalAxisRemoteValue.Value = angle4 - _step;
                    break;
            }
        }
    }

}
