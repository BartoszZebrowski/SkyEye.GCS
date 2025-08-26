using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SkyEye.UI.Common
{
    /// <summary>
    /// Klasa bazowa implementująca interfejs <see cref="INotifyPropertyChanged"/>.
    /// Umożliwia powiadamianie widoku o zmianach właściwości w modelu lub ViewModelu.
    /// </summary>
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        /// <summary>
        /// Zdarzenie wywoływane przy zmianie właściwości.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Powiadamia o zmianie wskazanej właściwości.
        /// Obsługuje wywołanie w głównym wątku aplikacji.
        /// </summary>
        /// <param name="propertyName">Nazwa zmienionej właściwości.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (Application.Current == null)
            {
                return;
            }
            if (Application.Current.Dispatcher.CheckAccess())
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                        () => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName))
                    )
                );
            }
        }

        /// <summary>
        /// Ustawia nową wartość pola i powiadamia o zmianie właściwości.
        /// </summary>
        /// <typeparam name="T">Typ wartości.</typeparam>
        /// <param name="field">Referencja do pola przechowującego wartość.</param>
        /// <param name="value">Nowa wartość.</param>
        /// <param name="propertyName">Nazwa zmienionej właściwości.</param>
        /// <returns>true, jeśli wartość została zmieniona; false w przeciwnym wypadku.</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }
    }

}
