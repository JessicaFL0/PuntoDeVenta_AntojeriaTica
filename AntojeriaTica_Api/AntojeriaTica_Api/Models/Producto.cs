namespace AntojeriaTica_Api.Models
{
    public class Producto
    {
        public int? IdProducto { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Existencias { get; set; }
    }

    public class ProductoHistory
    {
        public int IdProducto { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Cambio { get; set; } = string.Empty;
    }
}
