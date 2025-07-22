namespace AntojeriaTica_Web.Models
{
    public class MovimientoDiario
    {
        public int IdMovimiento { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoMovimiento { get; set; } = "";
        public string Categoria { get; set; } = "";
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = "";
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = "";

    }
}