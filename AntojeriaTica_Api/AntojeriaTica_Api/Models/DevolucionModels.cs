using System;
using System.Collections.Generic;

namespace AntojeriaTica_Api.Models
{
    public class Devolucion
    {
        public int Id { get; set; }
        public int VentaOriginalId { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoDevolucion { get; set; } = string.Empty; // 'Total', 'Parcial'
        public string TipoReembolso { get; set; } = string.Empty; // 'Efectivo', 'Tarjeta', 'Credito'
        public string MetodoPagoOriginal { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public decimal MontoDevuelto { get; set; }
        public string? Motivo { get; set; }
        public string Estado { get; set; } = "Procesada";
        public string? NumeroComprobante { get; set; }
        public List<DetalleDevolucion> Detalles { get; set; } = new List<DetalleDevolucion>();
    }

    public class DetalleDevolucion
    {
        public int Id { get; set; }
        public int DevolucionId { get; set; }
        public int ProductoId { get; set; }
        public int CantidadDevuelta { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalDevolucion { get; set; }
        public string? ProductoNombre { get; set; }
        public string? ProductoCodigo { get; set; }
    }

    public class CreditoCliente
    {
        public int Id { get; set; }
        public int DevolucionId { get; set; }
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public decimal MontoCredito { get; set; }
        public decimal MontoUtilizado { get; set; }
        public decimal SaldoDisponible { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = "Activo";
        public string? ComprobanteDevolucion { get; set; }
        public int VentaOriginalId { get; set; }
    }

    // Request Models
    public class DevolucionTotalRequest
    {
        public int VentaId { get; set; }
        public string TipoReembolso { get; set; } = string.Empty; // 'Efectivo', 'Tarjeta', 'Credito'
        public string? Motivo { get; set; }
        public string? NumeroIdentificacion { get; set; } // Solo para crédito
        public string? NombreCliente { get; set; } // Solo para crédito
        public int DiasVencimientoCredito { get; set; } = 90;
    }

    public class ProductoDevolucionRequest
    {
        public int ProductoId { get; set; }
        public int CantidadDevolver { get; set; }
    }

    public class DevolucionParcialRequest
    {
        public int VentaId { get; set; }
        public List<ProductoDevolucionRequest> ProductosDevolver { get; set; } = new List<ProductoDevolucionRequest>();
        public string TipoReembolso { get; set; } = string.Empty;
        public string? Motivo { get; set; }
        public string? NumeroIdentificacion { get; set; }
        public string? NombreCliente { get; set; }
        public int DiasVencimientoCredito { get; set; } = 90;
    }

    public class AplicarCreditoRequest
    {
        public int CreditoId { get; set; }
        public int VentaId { get; set; }
        public decimal MontoAplicar { get; set; }
    }

    // Response Models
    public class DevolucionResponse
    {
        public int Id { get; set; }
        public string? NumeroComprobante { get; set; }
        public decimal MontoDevuelto { get; set; }
        public string TipoReembolso { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int? CreditoId { get; set; }
        public bool Exitoso { get; set; } = true;
        public string? Mensaje { get; set; }
    }

    public class DevolucionDetallada
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
}
