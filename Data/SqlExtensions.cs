using System.Data;
using Microsoft.Data.SqlClient;

namespace WpfApp1.Data
{
    /// <summary>
    /// Métodos de apoyo para escribir los repositorios de forma más corta y clara.
    /// - AddParam: agrega un parámetro convirtiendo null de C# en DBNull (lo que SQL entiende).
    /// - GetXxx: lee una columna del SqlDataReader por su NOMBRE y controla los valores NULL.
    /// Se usa Convert.ToXxx porque tolera el tipo real de la columna (int, smallint, etc.).
    /// </summary>
    public static class SqlExtensions
    {
        // ---------- Parámetros de entrada ----------

        /// <summary>Agrega un parámetro al comando. Si el valor es null, envía DBNull.Value.</summary>
        public static SqlParameter AddParam(this SqlCommand cmd, string nombre, object? valor)
        {
            return cmd.Parameters.AddWithValue(nombre, valor ?? DBNull.Value);
        }

        // ---------- Lectura de columnas (con control de NULL) ----------

        public static int GetInt(this IDataRecord r, string columna)
            => Convert.ToInt32(r[columna]);

        public static int? GetIntOrNull(this IDataRecord r, string columna)
            => r[columna] == DBNull.Value ? null : Convert.ToInt32(r[columna]);

        public static short GetShort(this IDataRecord r, string columna)
            => Convert.ToInt16(r[columna]);

        public static decimal GetDecimal(this IDataRecord r, string columna)
            => Convert.ToDecimal(r[columna]);

        public static double GetDouble(this IDataRecord r, string columna)
            => Convert.ToDouble(r[columna]);

        public static bool GetBool(this IDataRecord r, string columna)
            => Convert.ToBoolean(r[columna]);

        /// <summary>Devuelve el texto de la columna, o cadena vacía si es NULL.</summary>
        public static string GetStringOrEmpty(this IDataRecord r, string columna)
            => r[columna] == DBNull.Value ? string.Empty : Convert.ToString(r[columna]) ?? string.Empty;

        /// <summary>Devuelve el texto de la columna, o null si es NULL.</summary>
        public static string? GetStringOrNull(this IDataRecord r, string columna)
            => r[columna] == DBNull.Value ? null : Convert.ToString(r[columna]);

        /// <summary>Devuelve la fecha de la columna, o null si es NULL.</summary>
        public static DateTime? GetDateOrNull(this IDataRecord r, string columna)
            => r[columna] == DBNull.Value ? null : Convert.ToDateTime(r[columna]);
    }
}
