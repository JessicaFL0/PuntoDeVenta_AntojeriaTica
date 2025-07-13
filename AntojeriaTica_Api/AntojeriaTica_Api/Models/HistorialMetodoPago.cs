using System;
using System.ComponentModel.DataAnnotations;

namespace AntojeriaTica_Api.Models
{
    public class HistorialMetodoPago
    {
        public int IdHistorial { get; set; }

        [Required]
        public int IdMetodoPago { get; set; }

        public DateTime FechaModificacion { get; set; }

        [Required]
        [StringLength(50)]
        public string Accion { get; set; }

        [Required]
        [StringLength(100)]
        public string UsuarioModificador { get; set; }
    }
}
