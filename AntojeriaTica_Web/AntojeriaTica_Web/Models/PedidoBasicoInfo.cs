using System;

namespace AntojeriaTica_Web.Models
{
    public class PedidoBasicoInfo
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
