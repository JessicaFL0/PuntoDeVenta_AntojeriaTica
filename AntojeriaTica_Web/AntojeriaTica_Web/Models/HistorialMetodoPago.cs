using System;

namespace AntojeriaTica_Web.Models
{
    public class HistorialMetodoPago
    {
        public int IdHistorial { get; set; }
        public int IdMetodoPago { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string Accion { get; set; }
        public string UsuarioModificador { get; set; }
    }
}
