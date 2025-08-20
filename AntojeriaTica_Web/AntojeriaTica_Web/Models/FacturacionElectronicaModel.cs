using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AntojeriaTica_Web.Models
{
    public class FacturaElectronicaModel
    {
        public int Id { get; set; }
        public string NumeroFactura { get; set; }
        public string ClaveNumerica { get; set; }
        
    [JsonPropertyName("nombreCliente")]
    public string ClienteNombre { get; set; }
        
    [JsonPropertyName("correoCliente")]
    public string ClienteEmail { get; set; }
        
        public DateTime FechaGeneracion { get; set; }
        public decimal TotalComprobante { get; set; }
        public string EstadoHacienda { get; set; }
        public bool EmailEnviado { get; set; }
        public int VentaId { get; set; }
        
        // Para compatibilidad con el código existente
        public int IdFactura => Id;
        public DateTime FechaEmision => FechaGeneracion;
        public decimal MontoTotal => TotalComprobante;
    }

    public class GenerarFacturaElectronicaViewModel
    {
        public int VentaId { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es obligatorio")]
        [Display(Name = "Nombre del Cliente")]
        public string ClienteNombre { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un email válido")]
        [Display(Name = "Email del Cliente")]
        public string ClienteEmail { get; set; }

        [Display(Name = "Teléfono del Cliente")]
        public string ClienteTelefono { get; set; }

        [Display(Name = "Identificación del Cliente")]
        public string ClienteIdentificacion { get; set; }
    }

    public class BusquedaFacturasElectronicasViewModel
    {
        [Display(Name = "Fecha Inicio")]
        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; }

        [Display(Name = "Fecha Fin")]
        [DataType(DataType.Date)]
        public DateTime? FechaFin { get; set; }

        [Display(Name = "Número de Factura")]
        public string NumeroFactura { get; set; }

        [Display(Name = "Nombre del Cliente")]
        public string ClienteNombre { get; set; }

        [Display(Name = "Estado Hacienda")]
        public string EstadoHacienda { get; set; }

        public List<FacturaElectronicaModel> Facturas { get; set; } = new List<FacturaElectronicaModel>();
    }

    public class FacturaElectronicaResponseModel
    {
        public int IdFactura { get; set; }
        public string NumeroFactura { get; set; }
        public string ClaveNumerica { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal SubTotal { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal MontoTotal { get; set; }
        public string EstadoHacienda { get; set; }
    }

    public class DetalleFacturaElectronicaModel
    {
        // Información general de la factura
        public int IdFactura { get; set; }
        public string NumeroFactura { get; set; }
        public string ClaveNumerica { get; set; }
        public string ClienteNombre { get; set; }
        public string ClienteEmail { get; set; }
        public string ClienteTelefono { get; set; }
        public string ClienteIdentificacion { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal SubTotal { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal MontoTotal { get; set; }
        public string EstadoHacienda { get; set; }
        public string MensajeHacienda { get; set; }
        public string RutaPDF { get; set; }
        public bool EmailEnviado { get; set; }
        public DateTime? FechaEnvioEmail { get; set; }
        public int VentaId { get; set; }
        public DateTime FechaVenta { get; set; }

        // Detalle de productos
        public List<DetalleProductoFacturaModel> Productos { get; set; } = new List<DetalleProductoFacturaModel>();

        // Historial de envíos
        public List<HistorialEnvioFacturaModel> HistorialEnvios { get; set; } = new List<HistorialEnvioFacturaModel>();
    }

    public class DetalleProductoFacturaModel
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubTotal { get; set; }
        public decimal PorcentajeImpuesto { get; set; }
        public decimal MontoImpuesto { get; set; }
    }

    public class HistorialEnvioFacturaModel
    {
        public int IdHistorial { get; set; }
        public string TipoEnvio { get; set; }
        public string Destinatario { get; set; }
        public string EstadoEnvio { get; set; }
        public string MensajeRespuesta { get; set; }
        public DateTime FechaEnvio { get; set; }
    }
}
