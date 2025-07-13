namespace AntojeriaTica_Web.Models
{
    public class VentaModel
    {
        public string MetodoPago { get; set; }
        public string CodigoCupon { get; set; }
        public List<DetalleVentaModel> Detalles { get; set; }
    }
}
