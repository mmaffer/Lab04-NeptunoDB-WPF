using System.Windows;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Ventana principal. Su única lógica es asignar el ViewModel de navegación
    /// como DataContext; todo lo demás lo resuelven los bindings del XAML.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // A partir de aquí, el XAML se enlaza a las propiedades de MainViewModel.
            DataContext = new MainViewModel();
        }
    }
}
