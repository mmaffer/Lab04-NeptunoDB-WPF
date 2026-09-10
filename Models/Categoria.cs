namespace WpfApp1.Models
{
    /// <summary>
    /// Representa una fila de la tabla Categorias (dbo.Categorias_Listar).
    /// </summary>
    public class Categoria
    {
        public int CategoriaID { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
