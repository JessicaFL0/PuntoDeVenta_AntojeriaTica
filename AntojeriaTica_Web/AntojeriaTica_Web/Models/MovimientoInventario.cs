namespace AntojeriaTica_Web.Models
{
    public class MovimientoInventario
    {
        public int IdProducto { get; set; }
        public string TipoMovimiento { get; set; } = "";
        public int Cantidad { get; set; }
        public int? CantidadEsperada { get; set; }
    }
}
