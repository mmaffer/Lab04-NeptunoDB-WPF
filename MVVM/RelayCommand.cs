using System.Windows.Input;

namespace WpfApp1.MVVM
{
    /// <summary>
    /// Implementación genérica de ICommand.
    /// Permite enlazar un botón del XAML (Command="{Binding GuardarCommand}")
    /// con un método del ViewModel, sin escribir code-behind.
    /// </summary>
    public class RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) : ICommand
    {
        /// <summary>
        /// Evento que avisa a WPF que vuelva a evaluar CanExecute.
        /// Se conecta al CommandManager para que se reevalúe solo
        /// cuando cambia el foco, se escribe en un TextBox, etc.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// WPF pregunta esto para saber si el control (botón) debe estar habilitado.
        /// Si no dimos condición, siempre está habilitado.
        /// </summary>
        public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;

        /// <summary>Se ejecuta cuando el usuario presiona el control.</summary>
        public void Execute(object? parameter) => execute(parameter);
    }
}
