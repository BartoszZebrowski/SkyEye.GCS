using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SkyEye.UI.Common
{
    /// <summary>
    /// Uniwersalna implementacja interfejsu <see cref="ICommand"/>.
    /// Umożliwia przekazanie logiki wykonania oraz warunku wykonania komendy
    /// w postaci delegatów.
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// Zdarzenie wywoływane przy zmianie stanu możliwości wykonania komendy.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        /// <summary>
        /// Konstruktor komendy.
        /// </summary>
        /// <param name="execute">Delegat z logiką wykonywania komendy.</param>
        /// <param name="canExecute">Delegat sprawdzający, czy komenda może być wykonana.</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Określa, czy komenda może być wykonana.
        /// </summary>
        /// <param name="parameter">Parametr przekazany do komendy.</param>
        /// <returns>true, jeśli komenda może być wykonana; w przeciwnym razie false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Wykonuje komendę.
        /// </summary>
        /// <param name="parameter">Parametr przekazany do komendy.</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

}
