using System;

namespace AntojeriaTica_Api.Models
{
    public class NotificacionPedido
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int UsuarioId { get; set; }
        public string TipoNotificacion { get; set; } = string.Empty; // Listo, Atrasado, Cancelado
        public string Mensaje { get; set; } = string.Empty;
        public bool Leida { get; set; } = false;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaLectura { get; set; }
        
        // Propiedades adicionales para el listado
        public string NumeroPedido { get; set; } = string.Empty;
        public string EstadoPedido { get; set; } = string.Empty;
        public string? Mesa { get; set; }
        public string? Cliente { get; set; }
    }
}
