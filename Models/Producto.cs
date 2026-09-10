namespace WpfApp1.Models
{
    /// <summary>
    /// Clase POCO (objeto simple) que representa una fila de la tabla Productos.
    /// Cada propiedad coincide con una columna devuelta por dbo.Productos_Listar.
    /// </summary>
    public class Producto
    {
        public int ProductoID { get; set; }
        public string NombreProducto { get; set; } = string.Empty;

        // Pueden ser NULL en la base de datos -> por eso son int? (nullable).
        public int? ProveedorID { get; set; }
        public int? CategoriaID { get; set; }

        public string? CantidadPorUnidad { get; set; }
        public decimal PrecioUnidad { get; set; }

        // En la tabla son smallint -> en C# los representamos como short.
        public short UnidadesEnExistencia { get; set; }
        public short UnidadesEnPedido { get; set; }
        public short NivelDeReorden { get; set; }

        public bool Descontinuado { get; set; }
    }
}
