namespace AntojeriaTica_Api.Models
{
    public class ActualizarEstadoPedidoRequest
    {
        public int PedidoId { get; set; }
        public string NuevoEstado { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
    }
}
