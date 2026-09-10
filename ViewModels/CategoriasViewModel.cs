using System.Collections.ObjectModel;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.MVVM;

namespace WpfApp1.ViewModels
{
    /// <summary>
    /// ViewModel de "Mantenimiento de Categorías".
    /// Mismo patrón que Productos, pero más simple: sólo Nombre y Descripción.
    /// El resultado de cada operación (éxito o error) se comunica a la vista
    /// mediante <see cref="MensajeEstado"/>, no con MessageBox.
    /// </summary>
    public class CategoriasViewModel : ViewModelBase
    {
        private readonly CategoriaRepository _repo = new();

        private Categoria _edicion = new();
        private Categoria? _seleccionada;
        private string _mensajeEstado = string.Empty;

        public CategoriasViewModel()
        {
            Categorias = new ObservableCollection<Categoria>();

            NuevoCommand = new RelayCommand(_ => PrepararNuevo());
            GuardarCommand = new RelayCommand(_ => Guardar());
            EliminarCommand = new RelayCommand(_ => Eliminar(), _ => Seleccionada != null);
            RefrescarCommand = new RelayCommand(_ => Cargar());
        }

        public ObservableCollection<Categoria> Categorias { get; }

        /// <summary>Categoría enlazada al formulario (alta o edición).</summary>
        public Categoria Edicion
        {
            get => _edicion;
            set => SetField(ref _edicion, value);
        }

        /// <summary>Fila seleccionada en la grilla.</summary>
        public Categoria? Seleccionada
        {
            get => _seleccionada;
            set
            {
                if (!SetField(ref _seleccionada, value) || value is null)
                    return;

                try
                {
                    // Releemos el registro desde la BD (EXEC dbo.Categorias_ObtenerPorId).
                    // Si el SP no lo encuentra, caemos a una copia de la fila de la grilla.
                    Edicion = _repo.ObtenerPorId(value.CategoriaID) ?? new Categoria
                    {
                        CategoriaID = value.CategoriaID,
                        NombreCategoria = value.NombreCategoria,
                        Descripcion = value.Descripcion
                    };
                    MensajeEstado = $"Editando categoría #{value.CategoriaID}.";
                }
                catch (Exception ex)
                {
                    MensajeEstado = "Error al cargar la categoría: " + ex.Message;
                }
            }
        }

        /// <summary>Mensaje de estado que la vista muestra (éxitos y errores).</summary>
        public string MensajeEstado
        {
            get => _mensajeEstado;
            private set => SetField(ref _mensajeEstado, value);
        }

        public RelayCommand NuevoCommand { get; }
        public RelayCommand GuardarCommand { get; }
        public RelayCommand EliminarCommand { get; }
        public RelayCommand RefrescarCommand { get; }

        /// <summary>Carga la lista de categorías desde dbo.Categorias_Listar.</summary>
        public void Cargar()
        {
            try
            {
                Categorias.Clear();
                foreach (var c in _repo.Listar())
                    Categorias.Add(c);

                MensajeEstado = $"{Categorias.Count} categoría(s) cargada(s).";
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error al cargar: " + ex.Message;
            }
        }

        private void PrepararNuevo()
        {
            Seleccionada = null;
            Edicion = new Categoria();
            MensajeEstado = "Nueva categoría: complete los datos y presione Guardar.";
        }

        private void Guardar()
        {
            try
            {
                if (Edicion.CategoriaID == 0)
                {
                    int nuevoId = _repo.Insertar(Edicion);
                    MensajeEstado = $"Categoría creada con ID {nuevoId}.";
                }
                else
                {
                    _repo.Actualizar(Edicion);
                    MensajeEstado = $"Categoría #{Edicion.CategoriaID} actualizada.";
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
            if (Seleccionada == null)
                return;

            try
            {
                int id = Seleccionada.CategoriaID;
                _repo.Eliminar(id);
                MensajeEstado = $"Categoría #{id} eliminada.";
                Cargar();
                PrepararNuevo();
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error al eliminar: " + ex.Message;
            }
        }
    }
}
