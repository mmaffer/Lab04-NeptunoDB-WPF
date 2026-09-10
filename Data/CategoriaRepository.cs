using System.Data;
using Microsoft.Data.SqlClient;
using WpfApp1.Models;

namespace WpfApp1.Data
{
    /// <summary>
    /// Acceso a datos de Categorías en modo CONECTADO: una conexión por método
    /// (using var connection = new SqlConnection(...); connection.Open();),
    /// siempre con CommandType.StoredProcedure y SqlParameter.
    /// </summary>
    public class CategoriaRepository
    {
        private readonly string _cadena = ConnectionHelper.DatabaseConnectionString;

        /// <summary>EXEC dbo.Categorias_Listar.</summary>
        public List<Categoria> Listar()
        {
            var lista = new List<Categoria>();

            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Categorias_Listar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(LeerCategoria(reader));
            }

            return lista;
        }

        /// <summary>EXEC dbo.Categorias_ObtenerPorId @CategoriaID.</summary>
        public Categoria? ObtenerPorId(int categoriaId)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Categorias_ObtenerPorId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.AddParam("@CategoriaID", categoriaId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? LeerCategoria(reader) : null;
        }

        /// <summary>EXEC dbo.Categorias_Insertar (con @NuevoID OUTPUT). Devuelve el id nuevo.</summary>
        public int Insertar(Categoria c)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Categorias_Insertar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.AddParam("@NombreCategoria", c.NombreCategoria);
            command.AddParam("@Descripcion", c.Descripcion);

            var pNuevoId = new SqlParameter("@NuevoID", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(pNuevoId);

            command.ExecuteNonQuery();

            return Convert.ToInt32(pNuevoId.Value);
        }

        /// <summary>EXEC dbo.Categorias_Actualizar.</summary>
        public void Actualizar(Categoria c)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Categorias_Actualizar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.AddParam("@CategoriaID", c.CategoriaID);
            command.AddParam("@NombreCategoria", c.NombreCategoria);
            command.AddParam("@Descripcion", c.Descripcion);

            command.ExecuteNonQuery();
        }

        /// <summary>EXEC dbo.Categorias_Eliminar @CategoriaID.</summary>
        public void Eliminar(int categoriaId)
        {
            using var connection = new SqlConnection(_cadena);
            connection.Open();

            using var command = new SqlCommand("dbo.Categorias_Eliminar", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.AddParam("@CategoriaID", categoriaId);

            command.ExecuteNonQuery();
        }

        private static Categoria LeerCategoria(IDataRecord reader)
        {
            return new Categoria
            {
                CategoriaID = reader.GetInt("CategoriaID"),
                NombreCategoria = reader.GetStringOrEmpty("NombreCategoria"),
                Descripcion = reader.GetStringOrNull("Descripcion")
            };
        }
    }
}
