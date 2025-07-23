using System;
using System.Collections.Generic;

namespace AntojeriaTica_Web.Models
{
    // Modelo general para devoluciones
    public class DevolucionModel
    {
        public int VentaId { get; set; }
        public VentaCompleta? VentaOriginal { get; set; }
        public string TipoDevolucion { get; set; } = "Total"; // Total o Parcial
        public string TipoReembolso { get; set; } = "Efectivo"; // Efectivo, Tarjeta, Credito
        public string Motivo { get; set; } = string.Empty;
        public string? NombreCliente { get; set; }
        public string? IdentificacionCliente { get; set; }
        public List<ProductoDevolucionModel> ProductosADevolver { get; set; } = new List<ProductoDevolucionModel>();
    }

    // Modelos de devolución para la web
    public class DevolucionTotalModel
    {
        public int VentaId { get; set; }
        public string TipoReembolso { get; set; } = "Efectivo";
        public string? Motivo { get; set; }
        public string? NumeroIdentificacion { get; set; }
        public string? NombreCliente { get; set; }
        public int DiasVencimientoCredito { get; set; } = 90;
        
        // Información de la venta original (para mostrar)
        public DateTime? FechaVenta { get; set; }
        public string? MetodoPagoOriginal { get; set; }
        public decimal MontoTotal { get; set; }
        public int CantidadProductos { get; set; }
        public bool VentaValida { get; set; }
    }

    public class ProductoDevolucionModel
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string ProductoCodigo { get; set; } = string.Empty;
        public int CantidadOriginal { get; set; }
        public int CantidadDevolver { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalDevolucion { get; set; }
        public bool Seleccionado { get; set; }
    }

    public class DevolucionParcialModel
    {
        public int VentaId { get; set; }
        public string TipoReembolso { get; set; } = "Efectivo";
        public string? Motivo { get; set; }
        public string? NumeroIdentificacion { get; set; }
        public string? NombreCliente { get; set; }
        public int DiasVencimientoCredito { get; set; } = 90;
        
        // Información de la venta original
        public DateTime? FechaVenta { get; set; }
        public string? MetodoPagoOriginal { get; set; }
        public decimal MontoTotalVenta { get; set; }
        
        // Productos disponibles para devolución
        public List<ProductoDevolucionModel> ProductosDisponibles { get; set; } = new List<ProductoDevolucionModel>();
        
        // Totales calculados
        public decimal MontoTotalDevolucion { get; set; }
        public int TotalProductosDevolver { get; set; }
    }

    public class CreditoClienteModel
    {
        public int Id { get; set; }
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public decimal MontoCredito { get; set; }
        public decimal MontoUtilizado { get; set; }
        public decimal SaldoDisponible { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? ComprobanteDevolucion { get; set; }
        public int VentaOriginalId { get; set; }
    }

    public class AplicarCreditoModel
    {
        public int CreditoId { get; set; }
        public int VentaId { get; set; }
        public decimal MontoAplicar { get; set; }
        public decimal SaldoDisponible { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string NumeroIdentificacion { get; set; } = string.Empty;
    }

    public class DevolucionDetalladaModel
    {
        public int Id { get; set; }
        public string? NumeroComprobante { get; set; }
        public int VentaOriginalId { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoDevolucion { get; set; } = string.Empty;
        public string TipoReembolso { get; set; } = string.Empty;
        public string MetodoPagoOriginal { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public decimal MontoDevuelto { get; set; }
        public string? Motivo { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaVentaOriginal { get; set; }
        public int CantidadProductosDevueltos { get; set; }
        public string? ClienteCredito { get; set; }
        public string? IdentificacionCliente { get; set; }
    }

    public class BusquedaDevolucionesModel
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? TipoDevolucion { get; set; }
        public string? TipoReembolso { get; set; }
        public List<DevolucionDetalladaModel> Resultados { get; set; } = new List<DevolucionDetalladaModel>();
        
        // Opciones para los filtros
        public List<string> TiposDevolucion { get; set; } = new List<string> { "Total", "Parcial" };
        public List<string> TiposReembolso { get; set; } = new List<string> { "Efectivo", "Tarjeta", "Credito" };
    }

    public class DevolucionResponseModel
    {
        public int Id { get; set; }
        public string? NumeroComprobante { get; set; }
        public decimal MontoDevuelto { get; set; }
        public string TipoReembolso { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int? CreditoId { get; set; }
        public bool Exitoso { get; set; }
        public string? Mensaje { get; set; }
    }

    public class BuscarCreditosModel
    {
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public List<CreditoClienteModel> CreditosDisponibles { get; set; } = new List<CreditoClienteModel>();
    }

    // Modelo para devoluciones realizadas (historial)
    public class DevolucionRealizada
    {
        public int Id { get; set; }
        public int VentaOriginalId { get; set; }
        public DateTime FechaDevolucion { get; set; }
        public string TipoDevolucion { get; set; } = string.Empty; // Total o Parcial
        public string TipoReembolso { get; set; } = string.Empty; // Efectivo, Tarjeta, Credito
        public decimal MontoDevuelto { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string? NombreCliente { get; set; }
        public string? IdentificacionCliente { get; set; }
        public string Estado { get; set; } = "Completada";
        
        // Información adicional de la venta original
        public DateTime FechaVentaOriginal { get; set; }
        public string MetodoPagoOriginal { get; set; } = string.Empty;
        public decimal MontoVentaOriginal { get; set; }
    }

    // Modelo para créditos de clientes (alias)
    public class CreditoCliente
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string IdentificacionCliente { get; set; } = string.Empty;
        public decimal MontoOriginal { get; set; }
        public decimal SaldoDisponible { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Estado { get; set; } = "Disponible"; // Disponible, Utilizado
        public int? DevolucionId { get; set; }
        
        // Información adicional
        public DateTime? FechaUltimoUso { get; set; }
        public string? DescripcionUltimoUso { get; set; }
    }
}
