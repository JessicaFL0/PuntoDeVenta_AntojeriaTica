namespace AntojeriaTica_Api.Models
{
    public class Descuento
    {
        public int IdDescuento { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; } 
        public decimal Valor { get; set; }
        public string? CodigoCupon { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; } 
        public string? Restricciones { get; set; }
    }
}
