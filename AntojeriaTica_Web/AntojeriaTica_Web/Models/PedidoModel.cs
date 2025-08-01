using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AntojeriaTica_Web.Models
{
    public class PedidoModel
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int UsuarioId { get; set; }
        
        [Display(Name = "Cliente")]
        public string? Cliente { get; set; }
        
        [Display(Name = "Mesa")]
        public string? Mesa { get; set; }
        
        [Required(ErrorMessage = "El tipo de pedido es requerido")]
        [Display(Name = "Tipo de Pedido")]
        public string TipoPedido { get; set; } = string.Empty;
        
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "En preparación";
        
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }
        
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        
        [Display(Name = "Detalles del Pedido")]
        public List<DetallePedidoModel> Detalles { get; set; } = new List<DetallePedidoModel>();
    }
}
