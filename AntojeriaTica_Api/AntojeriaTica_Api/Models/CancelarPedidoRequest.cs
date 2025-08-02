using System.ComponentModel.DataAnnotations;

namespace AntojeriaTica_Api.Models
{
    /// <summary>
    /// Modelo para solicitud de cancelación de pedidos - PED-004
    /// </summary>
    public class CancelarPedidoRequest
    {
        /// <summary>
        /// ID del pedido a cancelar
        /// </summary>
        [Required(ErrorMessage = "El ID del pedido es requerido")]
        public int PedidoId { get; set; }

        /// <summary>
        /// ID del usuario que solicita la cancelación
        /// </summary>
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int UsuarioId { get; set; }

        /// <summary>
        /// Motivo de la cancelación
        /// </summary>
        [Required(ErrorMessage = "El motivo de cancelación es requerido")]
        [StringLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
        public string MotivoCancelacion { get; set; } = string.Empty;

        /// <summary>
        /// ID del usuario administrador que autoriza la cancelación (opcional, requerido solo si el pedido ya inició preparación)
        /// </summary>
        public int? UsuarioAutorizacion { get; set; }
    }

    /// <summary>
    /// Modelo para respuesta de cancelación de pedidos
    /// </summary>
    public class CancelarPedidoResponse
    {
        /// <summary>
        /// Indica si la cancelación fue exitosa
        /// </summary>
        public bool Exitoso { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Indica si la cancelación requirió autorización
        /// </summary>
        public bool RequirioAutorizacion { get; set; }

        /// <summary>
        /// Tipo de cancelación realizada
        /// </summary>
        public string TipoCancelacion { get; set; } = string.Empty;

        /// <summary>
        /// Número del pedido cancelado
        /// </summary>
        public string NumeroPedido { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de la cancelación
        /// </summary>
        public DateTime FechaCancelacion { get; set; }
    }

    /// <summary>
    /// Modelo para verificar si un pedido puede ser cancelado
    /// </summary>
    public class VerificarCancelacionResponse
    {
        /// <summary>
        /// Indica si el pedido puede ser cancelado
        /// </summary>
        public bool PuedeCancelarse { get; set; }

        /// <summary>
        /// Mensaje explicativo
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Indica si requiere autorización de administrador
        /// </summary>
        public bool? RequiereAutorizacion { get; set; }

        /// <summary>
        /// Estado actual del pedido
        /// </summary>
        public string? EstadoActual { get; set; }

        /// <summary>
        /// Número del pedido
        /// </summary>
        public string? NumeroPedido { get; set; }
    }
}
