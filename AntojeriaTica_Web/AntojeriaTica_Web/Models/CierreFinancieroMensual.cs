namespace AntojeriaTica_Web.Models
{
    public class CierreFinancieroMensual
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
        public decimal TotalIngresos { get; set; } 
        public decimal TotalEgresos { get; set; }   
        public decimal UtilidadNeta { get; set; }   
        public DateTime FechaGeneracion { get; set; }
        public string GeneradoPor { get; set; }
        public string ComentarioJustificativo { get; set; }
    }
}