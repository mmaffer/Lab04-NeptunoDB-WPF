using System.Collections.ObjectModel;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.MVVM;

namespace WpfApp1.ViewModels
{
    /// <summary>
    /// ViewModel de la pantalla "Mantenimiento de Productos".
    /// Conecta la vista (ProductosView.xaml) con los repositorios (acceso a datos).
    /// El resultado de cada operación (éxito o error) se comunica a la vista
    /// mediante <see cref="MensajeEstado"/>, no con MessageBox.
    /// </summary>
    public class ProductosViewModel : ViewModelBase
    {
        // Repositorios: cada uno sabe hablar con sus procedimientos almacenados.
        private readonly ProductoRepository _productoRepo = new();
        private readonly CategoriaRepository _categoriaRepo = new();
        private readonly ProveedorRepository _proveedorRepo = new();

        private Producto _edicion = new();
        private Producto? _productoSeleccionado;
        private string _mensajeEstado = string.Empty;

        public ProductosViewModel()
        {
            // ObservableCollection avisa sola a la grilla cuando se agregan/quitan filas.
            Productos = new ObservableCollection<Producto>();
            Categorias = new ObservableCollection<Categoria>();
            Proveedores = new ObservableCollection<Proveedor>();

            // Comandos que usarán los botones de la vista.
            NuevoCommand = new RelayCommand(_ => PrepararNuevo());
            GuardarCommand = new RelayCommand(_ => Guardar());
            EliminarCommand = new RelayCommand(_ => Eliminar(), _ => ProductoSeleccionado != null);
            RefrescarCommand = new RelayCommand(_ => Cargar());
        }

        // ---------- Colecciones que se muestran en la vista ----------

        public ObservableCollection<Producto> Productos { get; }
        public ObservableCollection<Categoria> Categorias { get; }
        public ObservableCollection<Proveedor> Proveedores { get; }

        // ---------- Estado del formulario ----------

        /// <summary>Producto que se está creando o editando (enlazado al formulario).</summary>
        public Producto Edicion
        {
            get => _edicion;
            set => SetField(ref _edicion, value);
        }

        /// <summary>Fila seleccionada en la grilla. Al cambiar, cargamos sus datos al formulario.</summary>
        public Producto? ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set
            {
                if (!SetField(ref _productoSeleccionado, value) || value is null)
                    return;

                try
                {
                    // Releemos el producto desde la BD (EXEC dbo.Productos_ObtenerPorId).
                    // Si el SP no lo encuentra, trabajamos sobre una COPIA de la fila de la grilla.
                    Edicion = _productoRepo.ObtenerPorId(value.ProductoID) ?? Clonar(value);
                    MensajeEstado = $"Editando producto #{value.ProductoID}.";
                }
                catch (Exception ex)
                {
                    MensajeEstado = "Error al cargar el producto: " + ex.Message;
                }
            }
        }

        /// <summary>Mensaje de estado que la vista muestra (éxitos y errores).</summary>
        public string MensajeEstado
        {
            get => _mensajeEstado;
            private set => SetField(ref _mensajeEstado, value);
        }

        // ---------- Comandos ----------

        public RelayCommand NuevoCommand { get; }
        public RelayCommand GuardarCommand { get; }
        public RelayCommand EliminarCommand { get; }
        public RelayCommand RefrescarCommand { get; }

        // ---------- Lógica ----------

        /// <summary>
        /// Carga (o recarga) los productos y las listas de los ComboBox
        /// (Categorías y Proveedores). MainViewModel la llama cada vez que
        /// entramos a esta pantalla.
        /// </summary>
        public void Cargar()
        {
            try
            {
                // Listas para los ComboBox de Categoría y Proveedor.
                Categorias.Clear();
                foreach (var c in _categoriaRepo.Listar())
                    Categorias.Add(c);

                Proveedores.Clear();
                foreach (var p in _proveedorRepo.Listar())
                    Proveedores.Add(p);

                // Lista principal de productos.
                Productos.Clear();
                foreach (var prod in _productoRepo.Listar())
                    Productos.Add(prod);

                MensajeEstado = $"{Productos.Count} producto(s) cargado(s).";
            }
            catch (Exception ex)
            {
                // Nunca dejamos "reventar" la aplicación: mostramos el error en pantalla.
                MensajeEstado = "Error al cargar: " + ex.Message;
            }
        }

        /// <summary>Deja el formulario en blanco para registrar un producto nuevo.</summary>
        private void PrepararNuevo()
        {
            ProductoSeleccionado = null;
            Edicion = new Producto();   // ProductoID = 0 -> lo tomamos como "nuevo"
            MensajeEstado = "Nuevo producto: complete los datos y presione Guardar.";
        }

        /// <summary>Valida e inserta o actualiza según corresponda.</summary>
        private void Guardar()
        {
            // --- Validaciones solicitadas: nombre obligatorio y precio >= 0 ---
            if (string.IsNullOrWhiteSpace(Edicion.NombreProducto))
            {
                MensajeEstado = "El nombre del producto es obligatorio.";
                return;
            }

            if (Edicion.PrecioUnidad < 0)
            {
                MensajeEstado = "El precio unitario debe ser mayor o igual a 0.";
                return;
            }

            try
            {
                if (Edicion.ProductoID == 0)
                {
                    // No tiene id -> es un alta. El repositorio devuelve el id generado.
                    int nuevoId = _productoRepo.Insertar(Edicion);
                    MensajeEstado = $"Producto creado con ID {nuevoId}.";
                }
                else
                {
                    // Ya tiene id -> es una modificación.
                    _productoRepo.Actualizar(Edicion);
                    MensajeEstado = $"Producto #{Edicion.ProductoID} actualizado.";
                }

                Cargar();                 // refrescamos la grilla con los cambios
                PrepararNuevo();          // y dejamos el formulario limpio
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error al guardar: " + ex.Message;
            }
        }

        /// <summary>Elimina el producto seleccionado.</summary>
        private void Eliminar()
        {
            if (ProductoSeleccionado == null)
                return;

            try
            {
                int id = ProductoSeleccionado.ProductoID;
                _productoRepo.Eliminar(id);
                MensajeEstado = $"Producto #{id} eliminado.";
                Cargar();
                PrepararNuevo();
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error al eliminar: " + ex.Message;
            }
        }

        /// <summary>Copia todos los campos de un producto en un objeto nuevo.</summary>
        private static Producto Clonar(Producto o) => new()
        {
            ProductoID = o.ProductoID,
            NombreProducto = o.NombreProducto,
            ProveedorID = o.ProveedorID,
            CategoriaID = o.CategoriaID,
            CantidadPorUnidad = o.CantidadPorUnidad,
            PrecioUnidad = o.PrecioUnidad,
            UnidadesEnExistencia = o.UnidadesEnExistencia,
            UnidadesEnPedido = o.UnidadesEnPedido,
            NivelDeReorden = o.NivelDeReorden,
            Descontinuado = o.Descontinuado
        };
    }
}
