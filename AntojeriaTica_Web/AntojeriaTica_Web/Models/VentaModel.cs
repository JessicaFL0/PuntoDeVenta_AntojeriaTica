namespace AntojeriaTica_Web.Models
{
    public class VentaModel
    {
    public int? PedidoId { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string CodigoCupon { get; set; } = string.Empty;
    public List<DetalleVentaModel> Detalles { get; set; } = new List<DetalleVentaModel>();
    }
}
