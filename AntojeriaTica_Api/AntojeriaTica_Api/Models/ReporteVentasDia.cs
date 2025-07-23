using System;
using System.Collections.Generic;

namespace AntojeriaTica_Api.Models
{
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
}
