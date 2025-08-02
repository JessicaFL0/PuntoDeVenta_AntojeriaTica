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
        public int? TiempoEstimado { get; set; } // Tiempo estimado en minutos
        public int? TiempoPreparacion { get; set; } // Tiempo real de preparación en minutos
        public DateTime? FechaEstimadaEntrega { get; set; } // Fecha y hora estimada de entrega
        public bool EsAtrasado { get; set; } = false; // Indica si el pedido está atrasado
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public DateTime? FechaInicioPreparacion { get; set; } // Cuando empezó la preparación
        public DateTime? FechaFinalizacion { get; set; } // Cuando se completó el pedido
        
        // Campos para PED-004: Cancelación de pedidos
        public DateTime? FechaCancelacion { get; set; } // Cuando se canceló el pedido
        public string? MotivoCancelacion { get; set; } // Motivo de la cancelación
        public int? UsuarioCancelacion { get; set; } // Usuario que canceló el pedido
        public int? AutorizadoPor { get; set; } // Usuario administrador que autorizó la cancelación
        
        public List<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}
