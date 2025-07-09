namespace AntojeriaTica_Api.Models
{
    public class MovimientoInventario
    {
        public int IdProducto { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty; // Entrada o Salida
        public int Cantidad { get; set; }
    }

}
