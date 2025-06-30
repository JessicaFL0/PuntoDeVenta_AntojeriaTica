using System;
using System.Collections.Generic;

namespace AntojeriaTica_Api.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string MetodoPago { get; set; }
        public List<DetalleVenta> Detalles { get; set; }
    }
}
