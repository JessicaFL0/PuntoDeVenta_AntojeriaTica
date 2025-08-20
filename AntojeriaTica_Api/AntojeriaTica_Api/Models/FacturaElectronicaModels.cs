using System.ComponentModel.DataAnnotations;

namespace AntojeriaTica_Api.Models
{
    public class FacturaElectronica
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string ClaveNumerica { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; }
    public string TipoDocumento { get; set; } = "01";
        public string CodigoMoneda { get; set; } = "CRC";
        public decimal TipoCambio { get; set; } = 1.0m;
    public string EstadoHacienda { get; set; } = "Pendiente";
        public string? MensajeHacienda { get; set; }
        public DateTime? FechaRespuestaHacienda { get; set; }
        public string XmlFactura { get; set; } = string.Empty;
        public string? XmlRespuesta { get; set; }
        
    public string TipoIdentificacionCliente { get; set; } = "05";
        public string? IdentificacionCliente { get; set; }
        public string NombreCliente { get; set; } = "Cliente Contado";
        public string? CorreoCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        
        public decimal SubtotalServGravados { get; set; }
        public decimal SubtotalMercanciasGravadas { get; set; }
        public decimal SubtotalMercanciasExentas { get; set; }
        public decimal SubtotalServExentos { get; set; }
        public decimal MontoTotalMercanciasGravadas { get; set; }
        public decimal MontoTotalServGravados { get; set; }
        public decimal MontoTotalMercanciasExentas { get; set; }
        public decimal MontoTotalServExentos { get; set; }
        public decimal MontoTotalImpuesto { get; set; }
        public decimal TotalComprobante { get; set; }
        
        public string Estado { get; set; } = "Activo";
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }

    public class GenerarFacturaRequest
    {
        public int VentaId { get; set; }
        
        [Required]
        public string ClienteNombre { get; set; } = string.Empty;
        
        [EmailAddress]
        public string? ClienteEmail { get; set; }
        
        public string? ClienteTelefono { get; set; }
        
        public string? ClienteIdentificacion { get; set; }
    }

    public class ReenviarFacturaRequest
    {
        public int FacturaId { get; set; }
        
        [EmailAddress]
        public string CorreoDestino { get; set; } = string.Empty;
    }

    public class FacturaResponse
    {
        public int IdFactura { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string ClaveNumerica { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public decimal SubTotal { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal MontoTotal { get; set; }
        public string EstadoHacienda { get; set; } = string.Empty;
        public string? MensajeError { get; set; }
        public string? UrlDescarga { get; set; }
    }

    public class BusquedaFacturasModel
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? NumeroFactura { get; set; }
        public string? IdentificacionCliente { get; set; }
        public string? EstadoHacienda { get; set; }
        public List<FacturaResumen> Resultados { get; set; } = new List<FacturaResumen>();
    }

    public class FacturaResumen
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string ClaveNumerica { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string IdentificacionCliente { get; set; } = string.Empty;
        public decimal TotalComprobante { get; set; }
        public string EstadoHacienda { get; set; } = string.Empty;
        public string? MensajeHacienda { get; set; }
    }

    public class FacturaCompleta
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string ClaveNumerica { get; set; } = string.Empty;
        public DateTime FechaGeneracion { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string EstadoHacienda { get; set; } = string.Empty;
        public string? MensajeHacienda { get; set; }
        
        public string TipoIdentificacionCliente { get; set; } = string.Empty;
        public string IdentificacionCliente { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string? CorreoCliente { get; set; }
        
        public DateTime FechaVenta { get; set; }
        public string MetodoPagoVenta { get; set; } = string.Empty;
        
        public decimal SubtotalGravado { get; set; }
        public decimal SubtotalExento { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal TotalComprobante { get; set; }
        
        public List<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
    }

    public class DetalleFactura
    {
        public string CodigoProducto { get; set; } = string.Empty;
        public string DescripcionProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string UnidadMedida { get; set; } = "Unid";
        public decimal PrecioUnitario { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal BaseImponible { get; set; }
    public string TipoImpuesto { get; set; } = "01";
        public decimal TarifaImpuesto { get; set; }
        public decimal MontoImpuesto { get; set; }
    }

    public class ReenvioFacturaRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class DetalleFacturaElectronica
    {
        public int IdFactura { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string ClaveNumerica { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string? ClienteEmail { get; set; }
        public string? ClienteTelefono { get; set; }
        public string? ClienteIdentificacion { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal SubTotal { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal MontoTotal { get; set; }
        public string EstadoHacienda { get; set; } = string.Empty;
        public string? MensajeHacienda { get; set; }
        public bool EmailEnviado { get; set; }
        public DateTime? FechaEnvioEmail { get; set; }
        public int VentaId { get; set; }
        public DateTime FechaVenta { get; set; }
        
        public List<DetalleProductoFactura> Productos { get; set; } = new List<DetalleProductoFactura>();
        public List<HistorialEnvioFactura> HistorialEnvios { get; set; } = new List<HistorialEnvioFactura>();
    }

    public class DetalleProductoFactura
    {
        public int IdProducto { get; set; }
        public string? Codigo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubTotal { get; set; }
        public decimal PorcentajeImpuesto { get; set; }
        public decimal MontoImpuesto { get; set; }
    }

    public class HistorialEnvioFactura
    {
        public int IdHistorial { get; set; }
        public string TipoEnvio { get; set; } = string.Empty;
        public string? Destinatario { get; set; }
        public string EstadoEnvio { get; set; } = string.Empty;
        public string? MensajeRespuesta { get; set; }
        public DateTime FechaEnvio { get; set; }
    }
}
