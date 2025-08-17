namespace AntojeriaTica_Web.Models
{
    public class ReporteVentasAnualesResponse
    {
        public int Mes { get; set; }
        public string NombreMes { get; set; }
        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }
    }
}