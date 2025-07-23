using System;
using System.Collections.Generic;

namespace AntojeriaTica_Api.Models
{
    public class VentaCompleta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<DetalleVentaCompleto> Detalles { get; set; } = new List<DetalleVentaCompleto>();
    }
}
