using System.Collections.ObjectModel;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.MVVM;

namespace WpfApp1.ViewModels
{
    /// <summary>
    /// ViewModel de "Mantenimiento de Pedidos".
    /// Grilla + formulario (los IDs de Cliente/Empleado/Transportista se escriben
    /// directamente en TextBox numéricos) + eliminar, y una sección de reporte
    /// por rango de fechas (dbo.DetallePedidos_ListarPorFechas).
    /// El resultado de cada operación (éxito o error) se comunica a la vista
    /// mediante <see cref="MensajeEstado"/> / <see cref="MensajeEstadoReporte"/>, no con MessageBox.
    /// </summary>
    public class PedidosViewModel : ViewModelBase
    {
        private readonly PedidoRepository _repo = new();

        private Pedido _edicion = new() { FechaPedido = DateTime.Today };
        private Pedido? _seleccionado;
        private string _mensajeEstado = string.Empty;

        // Fechas del reporte (por defecto, el mes actual).
        private DateTime _reporteInicio = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        private DateTime _reporteFin = DateTime.Today;
        private string _mensajeEstadoReporte = string.Empty;

        public PedidosViewModel()
        {
            Pedidos = new ObservableCollection<Pedido>();
            Reporte = new ObservableCollection<DetallePedido>();

            NuevoCommand = new RelayCommand(_ => PrepararNuevo());
            GuardarCommand = new RelayCommand(_ => Guardar());
            EliminarCommand = new RelayCommand(_ => Eliminar(), _ => Seleccionado != null);
            RefrescarCommand = new RelayCommand(_ => Cargar());
            GenerarReporteCommand = new RelayCommand(_ => GenerarReporte());
        }

        // ---------- Colecciones ----------

        public ObservableCollection<Pedido> Pedidos { get; }
        public ObservableCollection<DetallePedido> Reporte { get; }

        // ---------- Formulario ----------

        public Pedido Edicion
        {
            get => _edicion;
            set => SetField(ref _edicion, value);
        }

        public Pedido? Seleccionado
        {
            get => _seleccionado;
            set
            {
                if (!SetField(ref _seleccionado, value) || value is null)
                    return;

                try
                {
                    // Releemos el pedido desde la BD (EXEC dbo.Pedidos_ObtenerPorId).
                    // Si el SP no lo encuentra, trabajamos sobre una COPIA de la fila de la grilla.
                    Edicion = _repo.ObtenerPorId(value.PedidoID) ?? Clonar(value);
                    MensajeEstado = $"Editando pedido #{value.PedidoID}.";
                }
                catch (Exception ex)
                {
                    MensajeEstado = "Error al cargar el pedido: " + ex.Message;
                }
            }
        }

        /// <summary>Mensaje de estado del mantenimiento que la vista muestra (éxitos y errores).</summary>
        public string MensajeEstado
        {
            get => _mensajeEstado;
            private set => SetField(ref _mensajeEstado, value);
        }

        // ---------- Reporte ----------

        public DateTime ReporteInicio
        {
            get => _reporteInicio;
            set => SetField(ref _reporteInicio, value);
        }

        public DateTime ReporteFin
        {
            get => _reporteFin;
            set => SetField(ref _reporteFin, value);
        }

        /// <summary>Mensaje de estado propio de la sección de reporte.</summary>
        public string MensajeEstadoReporte
        {
            get => _mensajeEstadoReporte;
            private set => SetField(ref _mensajeEstadoReporte, value);
        }

        // ---------- Comandos ----------

        public RelayCommand NuevoCommand { get; }
        public RelayCommand GuardarCommand { get; }
        public RelayCommand EliminarCommand { get; }
        public RelayCommand RefrescarCommand { get; }
        public RelayCommand GenerarReporteCommand { get; }

        // ---------- Lógica ----------

        /// <summary>Carga la lista de pedidos (EXEC dbo.Pedidos_Listar).</summary>
        public void Cargar()
        {
            try
            {
                Pedidos.Clear();
                foreach (var pe in _repo.Listar())
                    Pedidos.Add(pe);

                MensajeEstado = $"{Pedidos.Count} pedido(s) cargado(s).";
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error al cargar: " + ex.Message;
            }
        }

        private void PrepararNuevo()
        {
            Seleccionado = null;
            // Un pedido nuevo arranca con la fecha de hoy.
            Edicion = new Pedido { FechaPedido = DateTime.Today };
            MensajeEstado = "Nuevo pedido: complete los datos y presione Guardar.";
        }

        private void Guardar()
        {
            try
            {
                if (Edicion.PedidoID == 0)
                {
                    int nuevoId = _repo.Insertar(Edicion);
                    MensajeEstado = $"Pedido creado con ID {nuevoId}.";
                }
                else
                {
                    _repo.Actualizar(Edicion);
                    MensajeEstado = $"Pedido #{Edicion.PedidoID} actualizado.";
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
                int id = Seleccionado.PedidoID;
                _repo.Eliminar(id);
                MensajeEstado = $"Pedido #{id} eliminado.";
                Cargar();
                PrepararNuevo();
            }
            catch (Exception ex)
            {
                MensajeEstado = "Error al eliminar: " + ex.Message;
            }
        }

        /// <summary>
        /// Botón "Generar Reporte": llama a dbo.DetallePedidos_ListarPorFechas
        /// y llena la grilla del reporte (incluye SubTotal).
        /// </summary>
        private void GenerarReporte()
        {
            try
            {
                var filas = _repo.ListarDetallePorFechas(ReporteInicio, ReporteFin);

                Reporte.Clear();
                foreach (var f in filas)
                    Reporte.Add(f);

                MensajeEstadoReporte = $"{Reporte.Count} línea(s) en el rango de fechas.";
            }
            catch (Exception ex)
            {
                MensajeEstadoReporte = "Error al generar el reporte: " + ex.Message;
            }
        }

        private static Pedido Clonar(Pedido o) => new()
        {
            PedidoID = o.PedidoID,
            ClienteID = o.ClienteID,
            EmpleadoID = o.EmpleadoID,
            FechaPedido = o.FechaPedido,
            FechaRequerida = o.FechaRequerida,
            FechaEnvio = o.FechaEnvio,
            TransportistaID = o.TransportistaID,
            Destinatario = o.Destinatario,
            CiudadDestino = o.CiudadDestino,
            PaisDestino = o.PaisDestino
        };
    }
}
