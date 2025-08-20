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
    public string TipoPedido { get; set; } = string.Empty;
    public string Estado { get; set; } = "En preparación";
    public int? TiempoEstimado { get; set; }
    public int? TiempoPreparacion { get; set; }
    public DateTime? FechaEstimadaEntrega { get; set; }
    public bool EsAtrasado { get; set; } = false;
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaInicioPreparacion { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
        
    public DateTime? FechaCancelacion { get; set; }
    public string? MotivoCancelacion { get; set; }
    public int? UsuarioCancelacion { get; set; }
    public int? AutorizadoPor { get; set; }
        
        public List<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}
