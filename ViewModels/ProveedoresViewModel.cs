using System.Collections.ObjectModel;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.MVVM;

namespace WpfApp1.ViewModels
{
    /// <summary>
    /// ViewModel de "Mantenimiento de Proveedores".
    /// Igual que los demás (grilla + formulario + eliminar) y además
    /// un panel de búsqueda por Nombre de Contacto y Ciudad.
    /// El resultado de cada operación (éxito o error) se comunica a la vista
    /// mediante <see cref="MensajeEstado"/>, no con MessageBox.
    /// </summary>
    public class ProveedoresViewModel : ViewModelBase
    {
        private readonly ProveedorRepository _repo = new();

        private Proveedor _edicion = new();
        private Proveedor? _seleccionado;
        private string _mensajeEstado = string.Empty;

        // Campos del panel de búsqueda.
        private string _filtroContacto = string.Empty;
        private string _filtroCiudad = string.Empty;

        public ProveedoresViewModel()
        {
            Proveedores = new ObservableCollection<Proveedor>();

            NuevoCommand = new RelayCommand(_ => PrepararNuevo());
            GuardarCommand = new RelayCommand(_ => Guardar());
            EliminarCommand = new RelayCommand(_ => Eliminar(), _ => Seleccionado != null);
            BuscarCommand = new RelayCommand(_ => Buscar());
        }

        public ObservableCollection<Proveedor> Proveedores { get; }

        public Proveedor Edicion
        {
            get => _edicion;
            set => SetField(ref _edicion, value);
        }

        public Proveedor? Seleccionado
        {
            get => _seleccionado;
            set
            {
                if (!SetField(ref _seleccionado, value) || value is null)
                    return;

                try
                {
                    // Releemos el proveedor desde la BD (EXEC dbo.Proveedores_ObtenerPorId).
                    // Si el SP no lo encuentra, trabajamos sobre una COPIA de la fila de la grilla.
                    Edicion = _repo.ObtenerPorId(value.ProveedorID) ?? Clonar(value);
                    MensajeEstado = $"Editando proveedor #{value.ProveedorID}.";
                }
                catch (Exception ex)
                {
                    MensajeEstado = "Error al cargar el proveedor: " + ex.Message;
                }
            }
        }

        /// <summary>Mensaje de estado que la vista muestra (éxitos y errores).</summary>
        public string MensajeEstado
        {
            get => _mensajeEstado;
            private set => SetField(ref _mensajeEstado, value);
        }

        public string FiltroContacto
        {
            get => _filtroContacto;
            set => SetField(ref _filtroContacto, value);
        }

        public string FiltroCiudad
        {
            get => _filtroCiudad;
            set => SetField(ref _filtroCiudad, value);
        }

        public RelayCommand NuevoCommand { get; }
        public RelayCommand GuardarCommand { get; }
        public RelayCommand EliminarCommand { get; }
        public RelayCommand BuscarCommand { get; }

        /// <summary>Carga TODOS los proveedores (dbo.Proveedores_Listar).</summary>
        public void Cargar()
        {
            try
            {
                Proveedores.Clear();
                foreach (var p in _repo.Listar())
                    Proveedores.Add(p);

                MensajeEstado = $"{Proveedores.Count} proveedor(es) cargado(s).";
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error al cargar: " + ex.Message;
            }
        }

        /// <summary>
        /// Botón "Buscar". Si ambos filtros están vacíos, muestra todos (Listar).
        /// Si hay al menos uno, llama a dbo.Proveedores_BuscarPorContactoCiudad.
        /// </summary>
        private void Buscar()
        {
            try
            {
                bool sinFiltros = string.IsNullOrWhiteSpace(FiltroContacto)
                               && string.IsNullOrWhiteSpace(FiltroCiudad);

                var resultado = sinFiltros
                    ? _repo.Listar()
                    : _repo.BuscarPorContactoCiudad(FiltroContacto, FiltroCiudad);

                Proveedores.Clear();
                foreach (var p in resultado)
                    Proveedores.Add(p);

                MensajeEstado = sinFiltros
                    ? $"Sin filtros: {Proveedores.Count} proveedor(es)."
                    : $"Búsqueda: {Proveedores.Count} resultado(s).";
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error en la búsqueda: " + ex.Message;
            }
        }

        private void PrepararNuevo()
        {
            Seleccionado = null;
            Edicion = new Proveedor();
            MensajeEstado = "Nuevo proveedor: complete los datos y presione Guardar.";
        }

        private void Guardar()
        {
            try
            {
                if (Edicion.ProveedorID == 0)
                {
                    int nuevoId = _repo.Insertar(Edicion);
                    MensajeEstado = $"Proveedor creado con ID {nuevoId}.";
                }
                else
                {
                    _repo.Actualizar(Edicion);
                    MensajeEstado = $"Proveedor #{Edicion.ProveedorID} actualizado.";
                }

                Cargar();
                PrepararNuevo();
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error al guardar: " + ex.Message;
            }
        }

        private void Eliminar()
        {
            if (Seleccionado == null)
                return;

            try
            {
                int id = Seleccionado.ProveedorID;
                _repo.Eliminar(id);
                MensajeEstado = $"Proveedor #{id} eliminado.";
                Cargar();
                PrepararNuevo();
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error al eliminar: " + ex.Message;
            }
        }

        private static Proveedor Clonar(Proveedor o) => new()
        {
            ProveedorID = o.ProveedorID,
            CompaniaNombre = o.CompaniaNombre,
            NombreContacto = o.NombreContacto,
            CargoContacto = o.CargoContacto,
            Direccion = o.Direccion,
            Ciudad = o.Ciudad,
            CodigoPostal = o.CodigoPostal,
            Pais = o.Pais,
            Telefono = o.Telefono,
            Fax = o.Fax
        };
    }
}
