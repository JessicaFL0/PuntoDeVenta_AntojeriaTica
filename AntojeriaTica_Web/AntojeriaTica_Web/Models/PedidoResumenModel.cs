using System;
using System.ComponentModel.DataAnnotations;

namespace AntojeriaTica_Web.Models
{
    public class PedidoResumenModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Número de Pedido")]
        public string NumeroPedido { get; set; } = string.Empty;
        
        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; }
        
        [Display(Name = "Cliente")]
        public string? Cliente { get; set; }
        
        [Display(Name = "Mesa")]
        public string? Mesa { get; set; }
        
        [Display(Name = "Tipo")]
        public string TipoPedido { get; set; } = string.Empty;
        
        [Display(Name = "Estado")]
        public string Estado { get; set; } = string.Empty;
        
        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Total { get; set; }
        
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }
        
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;
        
        [Display(Name = "Items")]
        public int CantidadItems { get; set; }
    }
}
