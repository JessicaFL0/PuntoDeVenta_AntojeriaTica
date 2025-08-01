using AntojeriaTica_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System;

namespace AntojeriaTica_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PedidosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("RegistrarPedido")]
        public IActionResult RegistrarPedido([FromBody] Pedido pedido)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();

                    // Crear DataTable para los detalles del pedido
                    DataTable detallePedidoTable = new DataTable();
                    detallePedidoTable.Columns.Add("ProductoId", typeof(int));
                    detallePedidoTable.Columns.Add("Cantidad", typeof(int));
                    detallePedidoTable.Columns.Add("PrecioUnitario", typeof(decimal));
                    detallePedidoTable.Columns.Add("ObservacionesItem", typeof(string));

                    foreach (var detalle in pedido.Detalles)
                    {
                        detallePedidoTable.Rows.Add(detalle.ProductoId, detalle.Cantidad, detalle.PrecioUnitario, detalle.ObservacionesItem ?? "");
                    }

                    using (SqlCommand cmd = new SqlCommand("RegistrarPedido", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UsuarioId", pedido.UsuarioId);
                        cmd.Parameters.AddWithValue("@Cliente", pedido.Cliente ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Mesa", pedido.Mesa ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TipoPedido", pedido.TipoPedido);
                        cmd.Parameters.AddWithValue("@Observaciones", pedido.Observaciones ?? (object)DBNull.Value);

                        SqlParameter tvpParam = cmd.Parameters.AddWithValue("@DetallesPedido", detallePedidoTable);
                        tvpParam.SqlDbType = SqlDbType.Structured;
                        tvpParam.TypeName = "TipoDetallePedido";

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var resultado = new
                                {
                                    PedidoId = Convert.ToInt32(reader["PedidoId"]),
                                    NumeroPedido = reader["NumeroPedido"].ToString(),
                                    Mensaje = reader["Mensaje"].ToString()
                                };
                                return Ok(resultado);
                            }
                        }
                    }
                }

                return Ok(new { mensaje = "Pedido registrado correctamente" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("ActualizarEstado")]
        public IActionResult ActualizarEstadoPedido([FromBody] ActualizarEstadoPedidoRequest request)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("ActualizarEstadoPedido", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PedidoId", request.PedidoId);
                        cmd.Parameters.AddWithValue("@NuevoEstado", request.NuevoEstado);
                        cmd.Parameters.AddWithValue("@UsuarioId", request.UsuarioId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new { mensaje = reader["Mensaje"].ToString() });
                            }
                        }
                    }
                }

                return Ok(new { mensaje = "Estado actualizado correctamente" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("BuscarPedidos")]
        public IActionResult BuscarPedidos([FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? tipoPedido = null,
            [FromQuery] int? pedidoId = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("BuscarPedidos", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.HasValue ? (object)fechaInicio.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin.HasValue ? (object)fechaFin.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Estado", string.IsNullOrEmpty(estado) ? (object)DBNull.Value : estado);
                        cmd.Parameters.AddWithValue("@TipoPedido", string.IsNullOrEmpty(tipoPedido) ? (object)DBNull.Value : tipoPedido);
                        cmd.Parameters.AddWithValue("@PedidoId", pedidoId.HasValue ? (object)pedidoId.Value : DBNull.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var pedidos = new List<PedidoResumen>();

                            while (reader.Read())
                            {
                                pedidos.Add(new PedidoResumen
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    NumeroPedido = reader["NumeroPedido"]?.ToString() ?? string.Empty,
                                    Fecha = Convert.ToDateTime(reader["Fecha"]),
                                    Cliente = reader["Cliente"]?.ToString(),
                                    Mesa = reader["Mesa"]?.ToString(),
                                    TipoPedido = reader["TipoPedido"]?.ToString() ?? string.Empty,
                                    Estado = reader["Estado"]?.ToString() ?? string.Empty,
                                    Total = Convert.ToDecimal(reader["Total"]),
                                    Observaciones = reader["Observaciones"]?.ToString(),
                                    Usuario = reader["Usuario"]?.ToString() ?? string.Empty,
                                    CantidadItems = Convert.ToInt32(reader["CantidadItems"])
                                });
                            }

                            return Ok(pedidos);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("ObtenerDetalle/{pedidoId}")]
        public IActionResult ObtenerDetallePedido(int pedidoId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("ObtenerDetallePedido", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PedidoId", pedidoId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            Pedido? pedido = null;

                            // Leer información del pedido
                            if (reader.Read())
                            {
                                pedido = new Pedido
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    NumeroPedido = reader["NumeroPedido"]?.ToString() ?? string.Empty,
                                    Fecha = Convert.ToDateTime(reader["Fecha"]),
                                    Cliente = reader["Cliente"]?.ToString(),
                                    Mesa = reader["Mesa"]?.ToString(),
                                    TipoPedido = reader["TipoPedido"]?.ToString() ?? string.Empty,
                                    Estado = reader["Estado"]?.ToString() ?? string.Empty,
                                    Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                                    Impuesto = Convert.ToDecimal(reader["Impuesto"]),
                                    Total = Convert.ToDecimal(reader["Total"]),
                                    Observaciones = reader["Observaciones"]?.ToString()
                                };
                            }

                            // Leer detalles del pedido
                            if (reader.NextResult())
                            {
                                var detalles = new List<DetallePedido>();
                                while (reader.Read())
                                {
                                    detalles.Add(new DetallePedido
                                    {
                                        Id = Convert.ToInt32(reader["Id"]),
                                        ProductoId = Convert.ToInt32(reader["ProductoId"]),
                                        ProductoCodigo = reader["ProductoCodigo"]?.ToString(),
                                        ProductoNombre = reader["ProductoNombre"]?.ToString(),
                                        Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                        PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"]),
                                        Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                                        Impuesto = Convert.ToDecimal(reader["Impuesto"]),
                                        ObservacionesItem = reader["ObservacionesItem"]?.ToString()
                                    });
                                }

                                if (pedido != null)
                                {
                                    pedido.Detalles = detalles;
                                }
                            }

                            return Ok(pedido);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("ObtenerEstados")]
        public IActionResult ObtenerEstados()
        {
            var estados = new List<string>
            {
                "En preparación",
                "Listo",
                "Entregado",
                "Cancelado"
            };

            return Ok(estados);
        }

        [HttpGet("ObtenerTiposPedido")]
        public IActionResult ObtenerTiposPedido()
        {
            var tipos = new List<string>
            {
                "Mesa",
                "Telefono",
                "App"
            };

            return Ok(tipos);
        }
    }
}
