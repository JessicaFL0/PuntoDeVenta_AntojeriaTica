using System.ComponentModel.DataAnnotations;

namespace AntojeriaTica_Api.Models
{
    public class CancelarPedidoRequest
    {
        [Required(ErrorMessage = "El ID del pedido es requerido")]
        public int PedidoId { get; set; }
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int UsuarioId { get; set; }
        [Required(ErrorMessage = "El motivo de cancelación es requerido")]
        [StringLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
        public string MotivoCancelacion { get; set; } = string.Empty;
        public int? UsuarioAutorizacion { get; set; }
    }
    public class CancelarPedidoResponse
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public bool RequirioAutorizacion { get; set; }
        public string TipoCancelacion { get; set; } = string.Empty;
        public string NumeroPedido { get; set; } = string.Empty;
        public DateTime FechaCancelacion { get; set; }
    }
    public class VerificarCancelacionResponse
    {
        public bool PuedeCancelarse { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public bool? RequiereAutorizacion { get; set; }
        public string? EstadoActual { get; set; }
        public string? NumeroPedido { get; set; }
    }
}
