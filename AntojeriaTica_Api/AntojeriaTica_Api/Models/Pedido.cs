using System;
using System.Collections.Generic;

namespace AntojeriaTica_Api.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int UsuarioId { get; set; }
        public string? Cliente { get; set; }
        public string? Mesa { get; set; }
        public string TipoPedido { get; set; } = string.Empty; // Mesa, Telefono, App
        public string Estado { get; set; } = "En preparación"; // En preparación, Listo, Entregado, Cancelado
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public List<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}
