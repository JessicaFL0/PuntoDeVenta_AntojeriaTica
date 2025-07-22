namespace AntojeriaTica_Api.Models
{
    public class HistorialVenta
    {
        public int IdHistorial { get; set; }
        public int IdVenta { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string TipoOperacion { get; set; }
        public string UsuarioModificador { get; set; }
        public string DatosAntes { get; set; }
        public string DatosDespues { get; set; }
    }
}


public class HistorialDetalleVenta
{
    public int IdHistorial { get; set; }
    public int IdDetalleVenta { get; set; }
    public DateTime FechaModificacion { get; set; }
    public string TipoOperacion { get; set; } = null!;
    public string UsuarioModificador { get; set; } = null!;
    public string? DatosAntes { get; set; }
    public string? DatosDespues { get; set; }
}
