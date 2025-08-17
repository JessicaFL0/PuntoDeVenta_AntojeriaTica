namespace AntojeriaTica_Api.Models
{
    public class DashboardModel
    {
        public decimal TotalVentasHoy { get; set; }
        public int CantidadPedidosHoy { get; set; }
        public decimal TendenciaSemana { get; set; }
        public List<string> UltimosDias { get; set; } = new();
        public List<decimal> VentasUltimosDias { get; set; } = new();
    }
}
