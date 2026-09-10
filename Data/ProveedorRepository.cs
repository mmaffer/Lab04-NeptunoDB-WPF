using System.Data;
using Microsoft.Data.SqlClient;
using WpfApp1.Models;

namespace WpfApp1.Data
{
    /// <summary>
    /// Acceso a datos de Proveedores en modo CONECTADO: una conexión por método
    /// (using var connection = new SqlConnection(...); connection.Open();),
    /// siempre con CommandType.StoredProcedure y SqlParameter.
    /// Además del CRUD tiene una búsqueda por nombre de contacto y/o ciudad
    /// (dbo.Proveedores_BuscarPorContactoCiudad).
    /// </summary>
    public class ProveedorRepository
    {
        private readonly string _cadena = ConnectionHelper.DatabaseConnectionString;

        /// <summary>EXEC dbo.Proveedores_Listar.</summary>
        public List<Proveedor> Listar()
        {
            var lista = new List<Proveedor>();

            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Proveedores_Listar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(LeerProveedor(reader));
            }

            return lista;
        }

        /// <summary>
        /// EXEC dbo.Proveedores_BuscarPorContactoCiudad @NombreContacto, @Ciudad.
        /// Si un filtro viene vacío, enviamos NULL para que el SP lo ignore.
        /// </summary>
        public List<Proveedor> BuscarPorContactoCiudad(string? nombreContacto, string? ciudad)
        {
            var lista = new List<Proveedor>();

            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Proveedores_BuscarPorContactoCiudad", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // string.IsNullOrWhiteSpace -> true si es null, "" o sólo espacios.
            command.AddParam("@NombreContacto",
                string.IsNullOrWhiteSpace(nombreContacto) ? null : nombreContacto.Trim());
            command.AddParam("@Ciudad",
                string.IsNullOrWhiteSpace(ciudad) ? null : ciudad.Trim());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(LeerProveedor(reader));
            }

            return lista;
        }

        /// <summary>EXEC dbo.Proveedores_ObtenerPorId @ProveedorID.</summary>
        public Proveedor? ObtenerPorId(int proveedorId)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Proveedores_ObtenerPorId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.AddParam("@ProveedorID", proveedorId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? LeerProveedor(reader) : null;
        }

        /// <summary>EXEC dbo.Proveedores_Insertar (con @NuevoID OUTPUT). Devuelve el id nuevo.</summary>
        public int Insertar(Proveedor pr)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Proveedores_Insertar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            AgregarParametrosComunes(command, pr);

            var pNuevoId = new SqlParameter("@NuevoID", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(pNuevoId);

            command.ExecuteNonQuery();

            return Convert.ToInt32(pNuevoId.Value);
        }

        /// <summary>EXEC dbo.Proveedores_Actualizar.</summary>
        public void Actualizar(Proveedor pr)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Proveedores_Actualizar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.AddParam("@ProveedorID", pr.ProveedorID);
            AgregarParametrosComunes(command, pr);

            command.ExecuteNonQuery();
        }

        /// <summary>EXEC dbo.Proveedores_Eliminar @ProveedorID.</summary>
        public void Eliminar(int proveedorId)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Proveedores_Eliminar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.AddParam("@ProveedorID", proveedorId);

            command.ExecuteNonQuery();
        }

        /// <summary>Parámetros que Insertar y Actualizar comparten (para no repetir código).</summary>
        private static void AgregarParametrosComunes(SqlCommand command, Proveedor pr)
        {
            command.AddParam("@CompaniaNombre", pr.CompaniaNombre);
            command.AddParam("@NombreContacto", pr.NombreContacto);
            command.AddParam("@CargoContacto", pr.CargoContacto);
            command.AddParam("@Direccion", pr.Direccion);
            command.AddParam("@Ciudad", pr.Ciudad);
            command.AddParam("@CodigoPostal", pr.CodigoPostal);
            command.AddParam("@Pais", pr.Pais);
            command.AddParam("@Telefono", pr.Telefono);
            command.AddParam("@Fax", pr.Fax);
        }

        private static Proveedor LeerProveedor(IDataRecord reader)
        {
            return new Proveedor
            {
                ProveedorID = reader.GetInt("ProveedorID"),
                CompaniaNombre = reader.GetStringOrEmpty("CompaniaNombre"),
                NombreContacto = reader.GetStringOrNull("NombreContacto"),
                CargoContacto = reader.GetStringOrNull("CargoContacto"),
                Direccion = reader.GetStringOrNull("Direccion"),
                Ciudad = reader.GetStringOrNull("Ciudad"),
                CodigoPostal = reader.GetStringOrNull("CodigoPostal"),
                Pais = reader.GetStringOrNull("Pais"),
                Telefono = reader.GetStringOrNull("Telefono"),
                Fax = reader.GetStringOrNull("Fax")
            };
        }
    }
}
