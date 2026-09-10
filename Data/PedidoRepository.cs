using System.Data;
using Microsoft.Data.SqlClient;
using WpfApp1.Models;

namespace WpfApp1.Data
{
    /// <summary>
    /// Acceso a datos de Pedidos en modo CONECTADO: una conexión por método
    /// (using var connection = new SqlConnection(...); connection.Open();),
    /// siempre con CommandType.StoredProcedure y SqlParameter. Incluye:
    ///  - CRUD de la tabla Pedidos.
    ///  - El reporte dbo.DetallePedidos_ListarPorFechas (filtrado por rango de fechas).
    /// </summary>
    public class PedidoRepository
    {
        private readonly string _cadena = ConnectionHelper.DatabaseConnectionString;

        /// <summary>EXEC dbo.Pedidos_Listar.</summary>
        public List<Pedido> Listar()
        {
            var lista = new List<Pedido>();

            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Pedidos_Listar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(LeerPedido(reader));
            }

            return lista;
        }

        /// <summary>EXEC dbo.Pedidos_ObtenerPorId @PedidoID.</summary>
        public Pedido? ObtenerPorId(int pedidoId)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Pedidos_ObtenerPorId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.AddParam("@PedidoID", pedidoId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? LeerPedido(reader) : null;
        }

        /// <summary>EXEC dbo.Pedidos_Insertar (con @NuevoID OUTPUT). Devuelve el id nuevo.</summary>
        public int Insertar(Pedido pe)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Pedidos_Insertar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            AgregarParametrosComunes(command, pe);

            var pNuevoId = new SqlParameter("@NuevoID", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(pNuevoId);

            command.ExecuteNonQuery();

            return Convert.ToInt32(pNuevoId.Value);
        }

        /// <summary>EXEC dbo.Pedidos_Actualizar.</summary>
        public void Actualizar(Pedido pe)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Pedidos_Actualizar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.AddParam("@PedidoID", pe.PedidoID);
            AgregarParametrosComunes(command, pe);

            command.ExecuteNonQuery();
        }

        /// <summary>EXEC dbo.Pedidos_Eliminar @PedidoID.</summary>
        public void Eliminar(int pedidoId)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Pedidos_Eliminar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.AddParam("@PedidoID", pedidoId);

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Reporte: EXEC dbo.DetallePedidos_ListarPorFechas @FechaInicio, @FechaFin.
        /// Devuelve el detalle de pedidos dentro del rango de fechas, con SubTotal.
        /// </summary>
        public List<DetallePedido> ListarDetallePorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            var lista = new List<DetallePedido>();

            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.DetallePedidos_ListarPorFechas", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.AddParam("@FechaInicio", fechaInicio.Date);
            command.AddParam("@FechaFin", fechaFin.Date);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new DetallePedido
                {
                    PedidoID = reader.GetInt("PedidoID"),
                    FechaPedido = reader.GetDateOrNull("FechaPedido"),
                    Destinatario = reader.GetStringOrNull("Destinatario"),
                    CiudadDestino = reader.GetStringOrNull("CiudadDestino"),
                    PaisDestino = reader.GetStringOrNull("PaisDestino"),
                    ProductoID = reader.GetInt("ProductoID"),
                    NombreProducto = reader.GetStringOrEmpty("NombreProducto"),
                    PrecioUnidad = reader.GetDecimal("PrecioUnidad"),
                    Cantidad = reader.GetShort("Cantidad"),
                    Descuento = reader.GetDouble("Descuento"),
                    SubTotal = reader.GetDecimal("SubTotal")
                });
            }

            return lista;
        }

        /// <summary>Parámetros que Insertar y Actualizar comparten.</summary>
        private static void AgregarParametrosComunes(SqlCommand command, Pedido pe)
        {
            command.AddParam("@ClienteID", pe.ClienteID);
            command.AddParam("@EmpleadoID", pe.EmpleadoID);
            command.AddParam("@FechaPedido", pe.FechaPedido);       // requerida por el SP
            command.AddParam("@FechaRequerida", pe.FechaRequerida);
            command.AddParam("@FechaEnvio", pe.FechaEnvio);
            command.AddParam("@TransportistaID", pe.TransportistaID);
            command.AddParam("@Destinatario", pe.Destinatario);
            command.AddParam("@CiudadDestino", pe.CiudadDestino);
            command.AddParam("@PaisDestino", pe.PaisDestino);
        }

        private static Pedido LeerPedido(IDataRecord reader)
        {
            return new Pedido
            {
                PedidoID = reader.GetInt("PedidoID"),
                ClienteID = reader.GetIntOrNull("ClienteID"),
                EmpleadoID = reader.GetIntOrNull("EmpleadoID"),
                FechaPedido = reader.GetDateOrNull("FechaPedido"),
                FechaRequerida = reader.GetDateOrNull("FechaRequerida"),
                FechaEnvio = reader.GetDateOrNull("FechaEnvio"),
                TransportistaID = reader.GetIntOrNull("TransportistaID"),
                Destinatario = reader.GetStringOrNull("Destinatario"),
                CiudadDestino = reader.GetStringOrNull("CiudadDestino"),
                PaisDestino = reader.GetStringOrNull("PaisDestino")
            };
        }
    }
}
