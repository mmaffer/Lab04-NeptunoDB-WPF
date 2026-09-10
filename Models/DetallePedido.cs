namespace WpfApp1.Models
{
    /// <summary>
    /// Fila del reporte dbo.DetallePedidos_ListarPorFechas.
    /// No es exactamente una tabla: es el resultado de un JOIN entre
    /// Pedidos y DetallesDePedidos, incluyendo el SubTotal calculado.
    /// </summary>
    public class DetallePedido
    {
        public int PedidoID { get; set; }
        public DateTime? FechaPedido { get; set; }
        public string? Destinatario { get; set; }
        public string? CiudadDestino { get; set; }
        public string? PaisDestino { get; set; }

        public int ProductoID { get; set; }
        public string NombreProducto { get; set; } = string.Empty;

        public decimal PrecioUnidad { get; set; }
        public short Cantidad { get; set; }
        public double Descuento { get; set; }   // en la base es 'real' (float)
        public decimal SubTotal { get; set; }   // PrecioUnidad * Cantidad * (1 - Descuento)
    }
}
