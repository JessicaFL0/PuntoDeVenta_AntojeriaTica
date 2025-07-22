namespace AntojeriaTica_Api.Models
{
    public class HistorialFiltroRequest
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? TipoOperacion { get; set; }
        public string? Usuario { get; set; }
    }
}
