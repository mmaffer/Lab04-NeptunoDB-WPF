using WpfApp1.MVVM;

namespace WpfApp1.ViewModels
{
    /// <summary>
    /// ViewModel de la ventana principal. Su trabajo es la NAVEGACIÓN:
    /// mantener cuál ViewModel de pantalla está activo (CurrentViewModel) y
    /// cambiarlo cuando el usuario presiona un botón del menú.
    ///
    /// En MainWindow.xaml hay DataTemplates que dicen "para tal ViewModel,
    /// muestra tal UserControl", así que cambiar CurrentViewModel cambia la pantalla.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        // Creamos una sola instancia de cada pantalla y la reutilizamos.
        private readonly ProductosViewModel _productos = new();
        private readonly CategoriasViewModel _categorias = new();
        private readonly ProveedoresViewModel _proveedores = new();
        private readonly PedidosViewModel _pedidos = new();

        private object? _currentViewModel;

        public MainViewModel()
        {
            // Un solo comando recibe por parámetro el nombre de la pantalla a mostrar.
            NavegarCommand = new RelayCommand(destino => Navegar(destino as string));

            // Pantalla inicial.
            Navegar("Productos");
        }

        /// <summary>ViewModel de la pantalla que se está mostrando ahora.</summary>
        public object? CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetField(ref _currentViewModel, value);
        }

        /// <summary>Comando usado por todos los botones del menú lateral.</summary>
        public RelayCommand NavegarCommand { get; }

        /// <summary>Cambia la pantalla activa y le pide que cargue sus datos.</summary>
        private void Navegar(string? destino)
        {
            switch (destino)
            {
                case "Productos":
                    CurrentViewModel = _productos;
                    _productos.Cargar();
                    break;

                case "Categorias":
                    CurrentViewModel = _categorias;
                    _categorias.Cargar();
                    break;

                case "Proveedores":
                    CurrentViewModel = _proveedores;
                    _proveedores.Cargar();
                    break;

                case "Pedidos":
                    CurrentViewModel = _pedidos;
                    _pedidos.Cargar();
                    break;
            }
        }
    }
}
