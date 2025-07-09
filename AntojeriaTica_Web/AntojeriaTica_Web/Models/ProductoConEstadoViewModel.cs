namespace AntojeriaTica_Web.Models
{
    public class ProductoConEstadoViewModel
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Existencias { get; set; }
        public string EstadoStock { get; set; } = string.Empty;
    }
}
