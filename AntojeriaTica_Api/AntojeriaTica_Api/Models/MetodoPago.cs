using System.ComponentModel.DataAnnotations;

namespace AntojeriaTica_Api.Models
{
    public class MetodoPago
    {
        public int IdMetodoPago { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        public bool EstaActivo { get; set; }
    }
}
