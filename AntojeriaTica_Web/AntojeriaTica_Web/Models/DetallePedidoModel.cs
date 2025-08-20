using System.ComponentModel.DataAnnotations;

namespace AntojeriaTica_Web.Models
{
    public class DetallePedidoModel
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        
        [Required(ErrorMessage = "El producto es requerido")]
        [Display(Name = "Producto")]
        public int ProductoId { get; set; }
        
        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }
        
        [Required(ErrorMessage = "El precio unitario es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }
        
        public decimal Descuento { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Subtotal { get; set; }
        
        [Display(Name = "Observaciones del Item")]
        public string? ObservacionesItem { get; set; }
        
        [Display(Name = "Código")]
        public string? ProductoCodigo { get; set; }
        
        [Display(Name = "Nombre del Producto")]
        public string? ProductoNombre { get; set; }
    }
}
