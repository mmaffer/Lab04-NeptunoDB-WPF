using System.Data;
using Microsoft.Data.SqlClient;
using WpfApp1.Models;

namespace WpfApp1.Data
{
    /// <summary>
    /// Acceso a datos de Productos en modo CONECTADO: cada método abre su propia
    /// SqlConnection (con <c>using</c>, así se cierra sola), la abre, ejecuta un
    /// procedimiento almacenado y libera todo al terminar.
    /// Reglas que seguimos en cada método:
    ///   1) using var connection = new SqlConnection(...); connection.Open();
    ///   2) SqlCommand con CommandType.StoredProcedure (nunca SQL concatenado).
    ///   3) Un SqlParameter por cada parámetro del procedimiento.
    /// </summary>
    public class ProductoRepository
    {
        // Cadena de conexión leída desde App.config (vía ConnectionHelper).
        private readonly string _cadena = ConnectionHelper.DatabaseConnectionString;

        /// <summary>Trae todos los productos: EXEC dbo.Productos_Listar.</summary>
        public List<Producto> Listar()
        {
            var lista = new List<Producto>();

            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Productos_Listar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ExecuteReader porque el SP devuelve filas.
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(LeerProducto(reader));
            }

            return lista;
        }

        /// <summary>Trae un producto por su id: EXEC dbo.Productos_ObtenerPorId @ProductoID.</summary>
        public Producto? ObtenerPorId(int productoId)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Productos_ObtenerPorId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.AddParam("@ProductoID", productoId);

            using var reader = command.ExecuteReader();

            // Si hay fila, la leemos; si no, devolvemos null.
            return reader.Read() ? LeerProducto(reader) : null;
        }

        /// <summary>
        /// Inserta un producto y devuelve el ID nuevo.
        /// El SP tiene un parámetro @NuevoID de tipo OUTPUT: lo leemos DESPUÉS de ejecutar.
        /// </summary>
        public int Insertar(Producto p)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Productos_Insertar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Parámetros de entrada (en el mismo orden que el SP, aunque el orden no es obligatorio).
            command.AddParam("@NombreProducto", p.NombreProducto);
            command.AddParam("@ProveedorID", p.ProveedorID);
            command.AddParam("@CategoriaID", p.CategoriaID);
            command.AddParam("@CantidadPorUnidad", p.CantidadPorUnidad);
            command.AddParam("@PrecioUnidad", p.PrecioUnidad);
            command.AddParam("@UnidadesEnExistencia", p.UnidadesEnExistencia);
            command.AddParam("@UnidadesEnPedido", p.UnidadesEnPedido);
            command.AddParam("@NivelDeReorden", p.NivelDeReorden);
            command.AddParam("@Descontinuado", p.Descontinuado);

            // Parámetro de SALIDA: no lleva valor, pero indicamos Direction = Output.
            var pNuevoId = new SqlParameter("@NuevoID", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(pNuevoId);

            command.ExecuteNonQuery();          // no devuelve filas, sólo ejecuta

            // Ya ejecutado el SP, el parámetro OUTPUT tiene el ID generado.
            return Convert.ToInt32(pNuevoId.Value);
        }

        /// <summary>Actualiza un producto existente: EXEC dbo.Productos_Actualizar.</summary>
        public void Actualizar(Producto p)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Productos_Actualizar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.AddParam("@ProductoID", p.ProductoID);
            command.AddParam("@NombreProducto", p.NombreProducto);
            command.AddParam("@ProveedorID", p.ProveedorID);
            command.AddParam("@CategoriaID", p.CategoriaID);
            command.AddParam("@CantidadPorUnidad", p.CantidadPorUnidad);
            command.AddParam("@PrecioUnidad", p.PrecioUnidad);
            command.AddParam("@UnidadesEnExistencia", p.UnidadesEnExistencia);
            command.AddParam("@UnidadesEnPedido", p.UnidadesEnPedido);
            command.AddParam("@NivelDeReorden", p.NivelDeReorden);
            command.AddParam("@Descontinuado", p.Descontinuado);

            command.ExecuteNonQuery();
        }

        /// <summary>Elimina un producto por id: EXEC dbo.Productos_Eliminar @ProductoID.</summary>
        public void Eliminar(int productoId)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Productos_Eliminar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.AddParam("@ProductoID", productoId);

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Convierte la fila actual del reader en un objeto Producto.
        /// Se lee por NOMBRE de columna para no depender del orden.
        /// </summary>
        private static Producto LeerProducto(IDataRecord reader)
        {
            return new Producto
            {
                ProductoID = reader.GetInt("ProductoID"),
                NombreProducto = reader.GetStringOrEmpty("NombreProducto"),
                ProveedorID = reader.GetIntOrNull("ProveedorID"),
                CategoriaID = reader.GetIntOrNull("CategoriaID"),
                CantidadPorUnidad = reader.GetStringOrNull("CantidadPorUnidad"),
                PrecioUnidad = reader.GetDecimal("PrecioUnidad"),
                UnidadesEnExistencia = reader.GetShort("UnidadesEnExistencia"),
                UnidadesEnPedido = reader.GetShort("UnidadesEnPedido"),
                NivelDeReorden = reader.GetShort("NivelDeReorden"),
                Descontinuado = reader.GetBool("Descontinuado")
            };
        }
    }
}
