using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp1.MVVM
{
    /// <summary>
    /// Clase base para todos los ViewModels.
    /// Implementa INotifyPropertyChanged, que es el mecanismo que usa WPF
    /// para enterarse de que una propiedad cambió y refrescar la pantalla.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        // Evento que WPF escucha automáticamente cuando hay un Binding.
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Dispara la notificación para una propiedad.
        /// [CallerMemberName] hace que, si no pasamos nombre, tome el de la
        /// propiedad que llamó a este método.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Asigna un valor a un campo y, SÓLO si cambió, notifica a la vista.
        /// Uso típico:  set => SetField(ref _nombre, value);
        /// Devuelve true si el valor realmente cambió (útil para lógica extra).
        /// </summary>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
