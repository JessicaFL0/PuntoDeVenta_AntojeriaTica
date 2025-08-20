using System;
using System.Collections.Generic;

namespace AntojeriaTica_Web.Models
{
    public class BusquedaVentasModel
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? MetodoPago { get; set; }
        public int? VentaId { get; set; }
        public List<VentaDetallada> Resultados { get; set; } = new List<VentaDetallada>();
    public List<string> MetodosPago { get; set; } = new List<string> { "Efectivo", "Tarjeta", "SINPE Móvil" };
    }

    public class VentaDetallada
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int CantidadProductos { get; set; }
    }

    public class VentaCompleta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<DetalleVentaCompleto> Detalles { get; set; } = new List<DetalleVentaCompleto>();
    }

    public class DetalleVentaCompleto
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string ProductoCodigo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class ReporteVentasDia
    {
        public DateTime Fecha { get; set; }
        public int TotalVentas { get; set; }
        public decimal MontoTotal { get; set; }
        public List<VentasPorMetodo> VentasPorMetodo { get; set; } = new List<VentasPorMetodo>();
        public List<ProductoVendido> ProductosVendidos { get; set; } = new List<ProductoVendido>();
    }

    public class VentasPorMetodo
    {
        public string MetodoPago { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public decimal MontoTotal { get; set; }
    }

    public class ProductoVendido
    {
        public string ProductoCodigo { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal MontoTotal { get; set; }
    }

    // Modelo para validación de ventas
    public class ValidacionVentaModel
    {
        public bool EsValida { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public VentaCompleta? VentaInfo { get; set; }
        public bool YaDevuelta { get; set; }
        public decimal MontoTotal { get; set; }
    }
}
