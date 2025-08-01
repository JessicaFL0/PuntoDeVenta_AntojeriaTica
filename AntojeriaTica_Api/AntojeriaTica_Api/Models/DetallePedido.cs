namespace AntojeriaTica_Api.Models
{
    public class DetallePedido
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Subtotal { get; set; }
        public string? ObservacionesItem { get; set; }
        
        // Propiedades adicionales para mostrar información del producto
        public string? ProductoCodigo { get; set; }
        public string? ProductoNombre { get; set; }
    }
}
