using AntojeriaTica_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System;
using System.Linq;

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
                        cmd.Parameters.AddWithValue("@TiempoEstimado", pedido.TiempoEstimado ?? 30); // Default 30 minutos
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
                                    UsuarioId = pedido.UsuarioId,
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

        // Diagnóstico rápido para verificar UsuarioId grabado en Pedido
        [HttpGet("DebugUltimosPedidos")]
        public IActionResult DebugUltimosPedidos([FromQuery] int top = 10)
        {
            try
            {
                using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    var pedidos = new List<object>();
                    using (var cmd = new SqlCommand(@"SELECT TOP (@Top) p.Id, p.NumeroPedido, p.UsuarioId, p.Fecha, u.Nombre AS Usuario
                                                      FROM Pedido p LEFT JOIN Usuario u ON u.Id = p.UsuarioId
                                                      ORDER BY p.Id DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@Top", top);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pedidos.Add(new
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    NumeroPedido = reader.GetString(reader.GetOrdinal("NumeroPedido")),
                                    UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                                    Usuario = reader.IsDBNull(reader.GetOrdinal("Usuario")) ? null : reader.GetString(reader.GetOrdinal("Usuario")),
                                    Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha"))
                                });
                            }
                        }
                    }
                    return Ok(pedidos);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
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

        [HttpPost("ActualizarEstadoPedido/{pedidoId}")]
        public IActionResult ActualizarEstadoPedidoPorId(int pedidoId, [FromBody] ActualizarEstadoRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { 
                        success = false,
                        message = "El cuerpo de la petición es nulo" 
                    });
                }

                if (string.IsNullOrEmpty(request.NuevoEstado))
                {
                    return BadRequest(new { 
                        success = false,
                        message = "El nuevo estado es requerido" 
                    });
                }

                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("ActualizarEstadoPedido", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PedidoId", pedidoId);
                        cmd.Parameters.AddWithValue("@NuevoEstado", request.NuevoEstado);
                        cmd.Parameters.AddWithValue("@UsuarioId", request.UsuarioId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var mensaje = reader["Mensaje"]?.ToString() ?? "Estado actualizado";
                                return Ok(new { 
                                    success = true,
                                    message = mensaje 
                                });
                            }
                        }
                    }
                }

                return Ok(new { 
                    success = true,
                    message = "Estado actualizado correctamente" 
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { 
                    success = false,
                    message = $"Error: {ex.Message}" 
                });
            }
        }

        // Endpoint simplificado para testing
        [HttpPost("ActualizarEstadoSimple/{pedidoId}")]
        public IActionResult ActualizarEstadoSimple(int pedidoId, [FromBody] ActualizarEstadoRequest request)
        {
            try
            {
                Console.WriteLine($"API - ActualizarEstadoSimple - pedidoId: {pedidoId}");
                Console.WriteLine($"API - request is null: {request == null}");
                
                if (request != null)
                {
                    Console.WriteLine($"API - NuevoEstado: '{request.NuevoEstado}'");
                    Console.WriteLine($"API - UsuarioId: {request.UsuarioId}");
                }
                
                if (request == null || string.IsNullOrEmpty(request.NuevoEstado))
                {
                    Console.WriteLine("API - Datos inválidos detectados");
                    return BadRequest(new { 
                        success = false,
                        message = "Datos inválidos" 
                    });
                }

                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    
                    // Query SQL directo en lugar del stored procedure
                    string sql = @"
                        UPDATE Pedido 
                        SET Estado = @NuevoEstado, FechaActualizacion = GETDATE() 
                        WHERE Id = @PedidoId;
                        
                        SELECT 'Estado actualizado correctamente' as Mensaje;";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@PedidoId", pedidoId);
                        cmd.Parameters.AddWithValue("@NuevoEstado", request.NuevoEstado);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new { 
                                    success = true,
                                    message = reader["Mensaje"].ToString() 
                                });
                            }
                        }
                    }
                }

                return Ok(new { 
                    success = true,
                    message = "Estado actualizado correctamente" 
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { 
                    success = false,
                    message = $"Error: {ex.Message}" 
                });
            }
        }

        [HttpGet("BuscarPedidos")]
        public IActionResult BuscarPedidos([FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? tipoPedido = null,
            [FromQuery] int? pedidoId = null,
            [FromQuery] int? usuarioId = null,
            [FromQuery] bool soloAtrasados = false)
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
                        // Filtros adicionales (si el SP los soporta)
                        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@SoloAtrasados", soloAtrasados);

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

        // Información básica para validar edición (dueño, fecha, estado)
        [HttpGet("InfoBasica/{pedidoId}")]
        public IActionResult ObtenerInfoBasica(int pedidoId)
        {
            try
            {
                using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"SELECT Id, UsuarioId, Fecha, Estado FROM Pedido WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", pedidoId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                                    Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                                    Estado = reader.GetString(reader.GetOrdinal("Estado"))
                                });
                            }
                            return NotFound(new { message = "Pedido no encontrado" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class EditarBasicoRequest
        {
            public int UsuarioId { get; set; }
            public string? Cliente { get; set; }
            public string? Mesa { get; set; }
            public string? Observaciones { get; set; }
        }

        // Edición básica limitada a 5 minutos para el dueño; Admin/Cocina sin límite pero sin cambiar estado
        [HttpPut("EditarBasico/{pedidoId}")]
        public IActionResult EditarBasico(int pedidoId, [FromBody] EditarBasicoRequest request)
        {
            if (request == null || pedidoId <= 0 || request.UsuarioId <= 0)
            {
                return BadRequest(new { message = "Datos inválidos" });
            }

            try
            {
                using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();

                    // Traer datos del pedido
                    int ownerId = 0; DateTime fecha; string estado = "";
                    using (var cmd = new SqlCommand("SELECT UsuarioId, Fecha, Estado FROM Pedido WHERE Id=@Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", pedidoId);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read()) return NotFound(new { message = "Pedido no encontrado" });
                            ownerId = r.GetInt32(0); fecha = r.GetDateTime(1); estado = r.GetString(2);
                        }
                    }

                    // Regla de negocio: no editar si ya está Cancelado o Entregado
                    if (string.Equals(estado, "Cancelado", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(estado, "Entregado", StringComparison.OrdinalIgnoreCase))
                    {
                        return BadRequest(new { message = "No se puede editar un pedido cancelado o entregado" });
                    }

                    // Determinar si es admin/cocina a partir de la tabla Usuario.RolId (asumimos 1=Admin, 2=Cocina?)
                    // Si no existe esa convención, solo aplicar regla de 5 minutos al dueño.
                    bool esPrivilegiado = false;
                    using (var cmdRol = new SqlCommand(@"SELECT r.Nombre FROM Usuario u JOIN Rol r ON r.Id=u.RolId WHERE u.Id=@U", conn))
                    {
                        cmdRol.Parameters.AddWithValue("@U", request.UsuarioId);
                        var rolNombre = cmdRol.ExecuteScalar() as string;
                        esPrivilegiado = string.Equals(rolNombre, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(rolNombre, "Cocina", StringComparison.OrdinalIgnoreCase);
                    }

                    if (!esPrivilegiado)
                    {
                        var minutos = (int)(DateTime.Now - fecha).TotalMinutes;
                        if (request.UsuarioId != ownerId || minutos > 5)
                        {
                            return StatusCode(403, new { message = "No autorizado: ventana de edición expirada o no es el dueño" });
                        }
                    }

                    // Actualizar solo campos permitidos
                    using (var upd = new SqlCommand(@"UPDATE Pedido SET Cliente=@Cliente, Mesa=@Mesa, Observaciones=@Obs, FechaActualizacion=GETDATE() WHERE Id=@Id", conn))
                    {
                        upd.Parameters.AddWithValue("@Cliente", (object?)request.Cliente ?? DBNull.Value);
                        upd.Parameters.AddWithValue("@Mesa", (object?)request.Mesa ?? DBNull.Value);
                        upd.Parameters.AddWithValue("@Obs", (object?)request.Observaciones ?? DBNull.Value);
                        upd.Parameters.AddWithValue("@Id", pedidoId);
                        upd.ExecuteNonQuery();
                    }

                    return Ok(new { message = "Pedido actualizado" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public class EditarProductosRequest
        {
            public int UsuarioId { get; set; }
            public List<DetalleEditarProducto> Detalles { get; set; } = new List<DetalleEditarProducto>();
        }

        public class DetalleEditarProducto
        {
            public int ProductoId { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public string? ObservacionesItem { get; set; }
        }

        [HttpPut("EditarProductos/{pedidoId}")]
        public IActionResult EditarProductos(int pedidoId, [FromBody] EditarProductosRequest request)
        {
            if (request == null || pedidoId <= 0 || request.UsuarioId <= 0)
            {
                return BadRequest(new { message = "Datos inválidos" });
            }
            if (request.Detalles == null || request.Detalles.Count == 0)
            {
                return BadRequest(new { message = "Debe proporcionar al menos un producto" });
            }

            try
            {
                using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();

                    int ownerId = 0; DateTime fecha; string estado = "";
                    using (var cmd = new SqlCommand("SELECT UsuarioId, Fecha, Estado FROM Pedido WHERE Id=@Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", pedidoId);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read()) return NotFound(new { message = "Pedido no encontrado" });
                            ownerId = r.GetInt32(0); fecha = r.GetDateTime(1); estado = r.GetString(2);
                        }
                    }

                    if (string.Equals(estado, "Cancelado", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(estado, "Entregado", StringComparison.OrdinalIgnoreCase))
                    {
                        return BadRequest(new { message = "No se puede editar un pedido cancelado o entregado" });
                    }

                    bool esPrivilegiado = false;
                    using (var cmdRol = new SqlCommand(@"SELECT r.Nombre FROM Usuario u JOIN Rol r ON r.Id=u.RolId WHERE u.Id=@U", conn))
                    {
                        cmdRol.Parameters.AddWithValue("@U", request.UsuarioId);
                        var rolNombre = cmdRol.ExecuteScalar() as string;
                        esPrivilegiado = string.Equals(rolNombre, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(rolNombre, "Cocina", StringComparison.OrdinalIgnoreCase);
                    }

                    if (!esPrivilegiado)
                    {
                        var minutos = (int)(DateTime.Now - fecha).TotalMinutes;
                        if (request.UsuarioId != ownerId || minutos > 5)
                        {
                            return StatusCode(403, new { message = "No autorizado: ventana de edición expirada o no es el dueño" });
                        }
                    }

                    using (var tx = conn.BeginTransaction())
                    {
                        try
                        {
                            // Borrar detalles actuales
                            using (var del = new SqlCommand("DELETE FROM DetallePedido WHERE PedidoId=@Id", conn, tx))
                            {
                                del.Parameters.AddWithValue("@Id", pedidoId);
                                del.ExecuteNonQuery();
                            }

                            decimal subtotal = 0m; decimal impuesto = 0m;

                            foreach (var d in request.Detalles)
                            {
                                // Obtener si el producto es gravado
                                bool gravado = true;
                                using (var cmdG = new SqlCommand("SELECT ISNULL(Gravado,1) FROM Producto WHERE Id=@Pid", conn, tx))
                                {
                                    cmdG.Parameters.AddWithValue("@Pid", d.ProductoId);
                                    var val = cmdG.ExecuteScalar();
                                    if (val is bool b) gravado = b; else if (val is int i) gravado = i != 0; else gravado = true;
                                }

                                var subItem = d.Cantidad * d.PrecioUnitario;
                                var impItem = gravado ? subItem * 0.13m : 0m;

                                using (var ins = new SqlCommand(@"INSERT INTO DetallePedido (PedidoId, ProductoId, Cantidad, PrecioUnitario, Descuento, Impuesto, Subtotal, ObservacionesItem)
VALUES (@PedidoId, @ProductoId, @Cantidad, @PrecioUnitario, 0, @Impuesto, @Subtotal, @Obs)", conn, tx))
                                {
                                    ins.Parameters.AddWithValue("@PedidoId", pedidoId);
                                    ins.Parameters.AddWithValue("@ProductoId", d.ProductoId);
                                    ins.Parameters.AddWithValue("@Cantidad", d.Cantidad);
                                    ins.Parameters.AddWithValue("@PrecioUnitario", d.PrecioUnitario);
                                    ins.Parameters.AddWithValue("@Impuesto", impItem);
                                    ins.Parameters.AddWithValue("@Subtotal", subItem);
                                    ins.Parameters.AddWithValue("@Obs", (object?)d.ObservacionesItem ?? DBNull.Value);
                                    ins.ExecuteNonQuery();
                                }

                                subtotal += subItem;
                                impuesto += impItem;
                            }

                            using (var upd = new SqlCommand(@"UPDATE Pedido SET Subtotal=@S, Impuesto=@I, Total=@T, FechaActualizacion=GETDATE() WHERE Id=@Id", conn, tx))
                            {
                                upd.Parameters.AddWithValue("@S", subtotal);
                                upd.Parameters.AddWithValue("@I", impuesto);
                                upd.Parameters.AddWithValue("@T", subtotal + impuesto);
                                upd.Parameters.AddWithValue("@Id", pedidoId);
                                upd.ExecuteNonQuery();
                            }

                            tx.Commit();
                            return Ok(new { message = "Productos del pedido actualizados" });
                        }
                        catch (Exception ex)
                        {
                            tx.Rollback();
                            return StatusCode(500, new { message = ex.Message });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
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

        // PED-002: Endpoints para seguimiento de pedidos
        [HttpPost("DetectarPedidosAtrasados")]
        public IActionResult DetectarPedidosAtrasados()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DetectarPedidosAtrasados", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        var result = cmd.ExecuteScalar();
                        return Ok(new { PedidosAtrasadosDetectados = result, Mensaje = "Detección de pedidos atrasados completada" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("ObtenerNotificaciones/{usuarioId}")]
        public IActionResult ObtenerNotificaciones(int usuarioId, bool soloNoLeidas = true)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("ObtenerNotificacionesUsuario", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                        cmd.Parameters.AddWithValue("@SoloNoLeidas", soloNoLeidas);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var notificaciones = new List<NotificacionPedido>();
                            while (reader.Read())
                            {
                                notificaciones.Add(new NotificacionPedido
                                {
                                    Id = reader.GetInt32("Id"),
                                    PedidoId = reader.GetInt32("PedidoId"),
                                    NumeroPedido = reader.GetString("NumeroPedido"),
                                    TipoNotificacion = reader.GetString("TipoNotificacion"),
                                    Mensaje = reader.GetString("Mensaje"),
                                    Leida = reader.GetBoolean("Leida"),
                                    FechaCreacion = reader.GetDateTime("FechaCreacion"),
                                    FechaLectura = reader.IsDBNull("FechaLectura") ? null : reader.GetDateTime("FechaLectura"),
                                    EstadoPedido = reader.GetString("EstadoPedido"),
                                    Mesa = reader.IsDBNull("Mesa") ? null : reader.GetString("Mesa"),
                                    Cliente = reader.IsDBNull("Cliente") ? null : reader.GetString("Cliente")
                                });
                            }
                            return Ok(notificaciones);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("MarcarNotificacionLeida/{notificacionId}")]
        public IActionResult MarcarNotificacionLeida(int notificacionId, [FromBody] int usuarioId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("MarcarNotificacionLeida", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@NotificacionId", notificacionId);
                        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            return Ok(new { mensaje = reader.GetString("Mensaje") });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("ObtenerPedidosConSeguimiento")]
        public IActionResult ObtenerPedidosConSeguimiento(int? usuarioId = null, bool soloAtrasados = false)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("BuscarPedidos", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FechaInicio", DateTime.Today);
                        cmd.Parameters.AddWithValue("@FechaFin", DateTime.Today.AddDays(1));
                        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@SoloAtrasados", soloAtrasados);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var pedidos = new List<object>();
                            while (reader.Read())
                            {
                                pedidos.Add(new
                                {
                                    Id = reader.GetInt32("Id"),
                                    NumeroPedido = reader.GetString("NumeroPedido"),
                                    Fecha = reader.GetDateTime("Fecha"),
                                    Cliente = reader.IsDBNull("Cliente") ? null : reader.GetString("Cliente"),
                                    Mesa = reader.IsDBNull("Mesa") ? null : reader.GetString("Mesa"),
                                    TipoPedido = reader.GetString("TipoPedido"),
                                    Estado = reader.GetString("Estado"),
                                    Total = reader.GetDecimal("Total"),
                                    TiempoEstimado = reader.IsDBNull("TiempoEstimado") ? (int?)null : reader.GetInt32("TiempoEstimado"),
                                    TiempoPreparacion = reader.IsDBNull("TiempoPreparacion") ? (int?)null : reader.GetInt32("TiempoPreparacion"),
                                    FechaEstimadaEntrega = reader.IsDBNull("FechaEstimadaEntrega") ? (DateTime?)null : reader.GetDateTime("FechaEstimadaEntrega"),
                                    EsAtrasado = reader.GetBoolean("EsAtrasado"),
                                    Usuario = reader.GetString("Usuario"),
                                    CantidadItems = reader.GetInt32("CantidadItems"),
                                    TiempoTranscurrido = reader.GetInt32("TiempoTranscurrido"),
                                    EstadoTiempo = reader.GetString("EstadoTiempo"),
                                    MinutosDiferencia = reader.GetInt32("MinutosDiferencia")
                                });
                            }
                            return Ok(pedidos);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================================
        // ENDPOINTS PED-004: CANCELACIÓN DE PEDIDOS
        // =============================================

        /// <summary>
        /// Verificar si un pedido puede ser cancelado - PED-004
        /// </summary>
        /// <param name="pedidoId">ID del pedido a verificar</param>
        /// <returns>Información sobre la posibilidad de cancelación</returns>
        [HttpGet("VerificarCancelacion/{pedidoId}")]
        public IActionResult VerificarCancelacion(int pedidoId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("VerificarCancelacionPedido", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PedidoId", pedidoId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var response = new VerificarCancelacionResponse
                                {
                                    PuedeCancelarse = reader.GetBoolean("PuedeCancelarse"),
                                    Mensaje = reader.GetString("Mensaje"),
                                    RequiereAutorizacion = reader.IsDBNull("RequiereAutorizacion") ? (bool?)null : reader.GetBoolean("RequiereAutorizacion"),
                                    EstadoActual = reader.IsDBNull("EstadoActual") ? null : reader.GetString("EstadoActual"),
                                    NumeroPedido = reader.IsDBNull("NumeroPedido") ? null : reader.GetString("NumeroPedido")
                                };
                                return Ok(response);
                            }
                            return NotFound(new { message = "Pedido no encontrado" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cancelar un pedido - PED-004
        /// Escenario 1: Cancelación sin autorización (antes de iniciar preparación)
        /// Escenario 2: Cancelación con autorización (después de iniciar preparación)
        /// </summary>
        /// <param name="request">Datos de la cancelación</param>
        /// <returns>Resultado de la cancelación</returns>
        [HttpPost("CancelarPedido")]
        public IActionResult CancelarPedido([FromBody] CancelarPedidoRequest request)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("CancelarPedido", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PedidoId", request.PedidoId);
                        cmd.Parameters.AddWithValue("@UsuarioId", request.UsuarioId);
                        cmd.Parameters.AddWithValue("@MotivoCancelacion", request.MotivoCancelacion);
                        cmd.Parameters.AddWithValue("@UsuarioAutorizacion", request.UsuarioAutorizacion ?? (object)DBNull.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var response = new CancelarPedidoResponse
                                {
                                    Exitoso = true,
                                    Mensaje = reader.GetString("Mensaje"),
                                    RequirioAutorizacion = reader.GetBoolean("RequirioAutorizacion"),
                                    TipoCancelacion = reader.GetString("TipoCancelacion"),
                                    NumeroPedido = reader.GetString("NumeroPedido"),
                                    FechaCancelacion = reader.GetDateTime("FechaCancelacion")
                                };
                                return Ok(response);
                            }
                            return StatusCode(500, new { message = "Error inesperado en la cancelación" });
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Errores específicos de SQL Server (validaciones de negocio)
                return BadRequest(new CancelarPedidoResponse
                {
                    Exitoso = false,
                    Mensaje = sqlEx.Message,
                    RequirioAutorizacion = false,
                    TipoCancelacion = "Error",
                    NumeroPedido = "",
                    FechaCancelacion = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener historial de cancelaciones
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio opcional</param>
        /// <param name="fechaFin">Fecha de fin opcional</param>
        /// <param name="usuarioId">ID del usuario opcional</param>
        /// <returns>Lista de pedidos cancelados</returns>
        [HttpGet("HistorialCancelaciones")]
        public IActionResult ObtenerHistorialCancelaciones(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? usuarioId = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("BuscarPedidos", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Estado", "Cancelado");
                        cmd.Parameters.AddWithValue("@TipoPedido", DBNull.Value);
                        cmd.Parameters.AddWithValue("@PedidoId", DBNull.Value);
                        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@SoloAtrasados", false);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var pedidosCancelados = new List<object>();
                            while (reader.Read())
                            {
                                pedidosCancelados.Add(new
                                {
                                    Id = reader.GetInt32("Id"),
                                    NumeroPedido = reader.GetString("NumeroPedido"),
                                    Fecha = reader.GetDateTime("Fecha"),
                                    Cliente = reader.IsDBNull("Cliente") ? null : reader.GetString("Cliente"),
                                    Mesa = reader.IsDBNull("Mesa") ? null : reader.GetString("Mesa"),
                                    TipoPedido = reader.GetString("TipoPedido"),
                                    Estado = reader.GetString("Estado"),
                                    Total = reader.GetDecimal("Total"),
                                    FechaCancelacion = reader.IsDBNull("FechaCancelacion") ? (DateTime?)null : reader.GetDateTime("FechaCancelacion"),
                                    MotivoCancelacion = reader.IsDBNull("MotivoCancelacion") ? null : reader.GetString("MotivoCancelacion"),
                                    Usuario = reader.GetString("Usuario"),
                                    UsuarioCancelacion = reader.IsDBNull("UsuarioCancelacion") ? null : reader.GetString("UsuarioCancelacion"),
                                    AutorizadoPor = reader.IsDBNull("AutorizadoPor") ? null : reader.GetString("AutorizadoPor")
                                });
                            }
                            return Ok(pedidosCancelados);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
