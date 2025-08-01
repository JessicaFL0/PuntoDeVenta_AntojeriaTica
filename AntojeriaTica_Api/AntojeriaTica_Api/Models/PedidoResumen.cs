using System;

namespace AntojeriaTica_Api.Models
{
    public class PedidoResumen
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string? Cliente { get; set; }
        public string? Mesa { get; set; }
        public string TipoPedido { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public int CantidadItems { get; set; }
    }
}
