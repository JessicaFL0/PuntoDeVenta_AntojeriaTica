using System;

namespace AntojeriaTica_Api.Models
{
    public class VentaDetallada
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int CantidadProductos { get; set; }
    }
}
