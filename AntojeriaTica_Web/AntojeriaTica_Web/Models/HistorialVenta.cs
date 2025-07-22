namespace AntojeriaTica_Web.Models
{
    public class HistorialVenta
    {
        public int IdHistorial { get; set; }
        public int IdVenta { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string TipoOperacion { get; set; }
        public string UsuarioModificador { get; set; }
        public string? DatosAntes { get; set; }
        public string? DatosDespues { get; set; }
    }
}
