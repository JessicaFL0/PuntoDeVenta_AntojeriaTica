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
    public class DevolucionesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DevolucionesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("ProcesarDevolucionTotal")]
        public IActionResult ProcesarDevolucionTotal([FromBody] DevolucionTotalRequest request)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("ProcesarDevolucionTotal", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        
                        cmd.Parameters.AddWithValue("@VentaId", request.VentaId);
                        cmd.Parameters.AddWithValue("@TipoReembolso", request.TipoReembolso);
                        cmd.Parameters.AddWithValue("@Motivo", string.IsNullOrEmpty(request.Motivo) ? (object)DBNull.Value : request.Motivo);
                        cmd.Parameters.AddWithValue("@NumeroIdentificacion", string.IsNullOrEmpty(request.NumeroIdentificacion) ? (object)DBNull.Value : request.NumeroIdentificacion);
                        cmd.Parameters.AddWithValue("@NombreCliente", string.IsNullOrEmpty(request.NombreCliente) ? (object)DBNull.Value : request.NombreCliente);
                        cmd.Parameters.AddWithValue("@DiasVencimientoCredito", request.DiasVencimientoCredito);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var response = new DevolucionResponse
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    NumeroComprobante = reader["NumeroComprobante"]?.ToString(),
                                    MontoDevuelto = Convert.ToDecimal(reader["MontoDevuelto"]),
                                    TipoReembolso = reader["TipoReembolso"]?.ToString() ?? string.Empty,
                                    Fecha = Convert.ToDateTime(reader["Fecha"]),
                                    CreditoId = reader["CreditoId"] == DBNull.Value ? null : Convert.ToInt32(reader["CreditoId"]),
                                    Exitoso = true,
                                    Mensaje = "Devolución total procesada correctamente"
                                };
                                
                                return Ok(response);
                            }
                        }
                    }
                }
                
                return BadRequest(new { error = "No se pudo procesar la devolución" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { 
                    error = ex.Message,
                    ventaId = request.VentaId,
                    tipoReembolso = request.TipoReembolso
                });
            }
        }

        [HttpPost("ProcesarDevolucionParcial")]
        public IActionResult ProcesarDevolucionParcial([FromBody] DevolucionParcialRequest request)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();

                    // Crear tabla de productos para devolver
                    DataTable productosTable = new DataTable();
                    productosTable.Columns.Add("ProductoId", typeof(int));
                    productosTable.Columns.Add("CantidadDevolver", typeof(int));

                    foreach (var producto in request.ProductosDevolver)
                    {
                        productosTable.Rows.Add(producto.ProductoId, producto.CantidadDevolver);
                    }

                    using (SqlCommand cmd = new SqlCommand("ProcesarDevolucionParcial", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        
                        cmd.Parameters.AddWithValue("@VentaId", request.VentaId);
                        
                        SqlParameter tvpParam = cmd.Parameters.AddWithValue("@ProductosDevolver", productosTable);
                        tvpParam.SqlDbType = SqlDbType.Structured;
                        tvpParam.TypeName = "TipoProductosDevolucion";
                        
                        cmd.Parameters.AddWithValue("@TipoReembolso", request.TipoReembolso);
                        cmd.Parameters.AddWithValue("@Motivo", string.IsNullOrEmpty(request.Motivo) ? (object)DBNull.Value : request.Motivo);
                        cmd.Parameters.AddWithValue("@NumeroIdentificacion", string.IsNullOrEmpty(request.NumeroIdentificacion) ? (object)DBNull.Value : request.NumeroIdentificacion);
                        cmd.Parameters.AddWithValue("@NombreCliente", string.IsNullOrEmpty(request.NombreCliente) ? (object)DBNull.Value : request.NombreCliente);
                        cmd.Parameters.AddWithValue("@DiasVencimientoCredito", request.DiasVencimientoCredito);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var response = new DevolucionResponse
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    NumeroComprobante = reader["NumeroComprobante"]?.ToString(),
                                    MontoDevuelto = Convert.ToDecimal(reader["MontoDevuelto"]),
                                    TipoReembolso = reader["TipoReembolso"]?.ToString() ?? string.Empty,
                                    Fecha = Convert.ToDateTime(reader["Fecha"]),
                                    CreditoId = reader["CreditoId"] == DBNull.Value ? null : Convert.ToInt32(reader["CreditoId"]),
                                    Exitoso = true,
                                    Mensaje = "Devolución parcial procesada correctamente"
                                };
                                
                                return Ok(response);
                            }
                        }
                    }
                }
                
                return BadRequest(new { error = "No se pudo procesar la devolución parcial" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { 
                    error = ex.Message,
                    ventaId = request.VentaId,
                    tipoReembolso = request.TipoReembolso
                });
            }
        }

        [HttpGet("BuscarCreditosCliente/{numeroIdentificacion}")]
        public IActionResult BuscarCreditosCliente(string numeroIdentificacion)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("BuscarCreditosCliente", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@NumeroIdentificacion", numeroIdentificacion);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var creditos = new List<CreditoCliente>();
                            
                            while (reader.Read())
                            {
                                creditos.Add(new CreditoCliente
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    NumeroIdentificacion = reader["NumeroIdentificacion"]?.ToString() ?? string.Empty,
                                    NombreCliente = reader["NombreCliente"]?.ToString() ?? string.Empty,
                                    MontoCredito = Convert.ToDecimal(reader["MontoCredito"]),
                                    MontoUtilizado = Convert.ToDecimal(reader["MontoUtilizado"]),
                                    SaldoDisponible = Convert.ToDecimal(reader["SaldoDisponible"]),
                                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                                    FechaVencimiento = Convert.ToDateTime(reader["FechaVencimiento"]),
                                    Estado = reader["Estado"]?.ToString() ?? string.Empty,
                                    ComprobanteDevolucion = reader["ComprobanteDevolucion"]?.ToString(),
                                    VentaOriginalId = Convert.ToInt32(reader["VentaOriginalId"])
                                });
                            }
                            
                            return Ok(creditos);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("AplicarCredito")]
        public IActionResult AplicarCredito([FromBody] AplicarCreditoRequest request)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("AplicarCreditoAVenta", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        
                        cmd.Parameters.AddWithValue("@CreditoId", request.CreditoId);
                        cmd.Parameters.AddWithValue("@VentaId", request.VentaId);
                        cmd.Parameters.AddWithValue("@MontoAplicar", request.MontoAplicar);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new {
                                    mensaje = reader["Mensaje"]?.ToString(),
                                    montoAplicado = Convert.ToDecimal(reader["MontoAplicado"]),
                                    saldoRestante = Convert.ToDecimal(reader["SaldoRestante"])
                                });
                            }
                        }
                    }
                }
                
                return BadRequest(new { error = "No se pudo aplicar el crédito" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("HistorialDevoluciones")]
        public IActionResult ConsultarHistorialDevoluciones([FromQuery] DateTime? fechaInicio = null, 
            [FromQuery] DateTime? fechaFin = null, 
            [FromQuery] string? tipoDevolucion = null, 
            [FromQuery] string? tipoReembolso = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("ConsultarHistorialDevoluciones", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.HasValue ? (object)fechaInicio.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin.HasValue ? (object)fechaFin.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@TipoDevolucion", string.IsNullOrEmpty(tipoDevolucion) ? (object)DBNull.Value : tipoDevolucion);
                        cmd.Parameters.AddWithValue("@TipoReembolso", string.IsNullOrEmpty(tipoReembolso) ? (object)DBNull.Value : tipoReembolso);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var devoluciones = new List<DevolucionDetallada>();
                            
                            while (reader.Read())
                            {
                                devoluciones.Add(new DevolucionDetallada
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    NumeroComprobante = reader["NumeroComprobante"]?.ToString(),
                                    VentaOriginalId = Convert.ToInt32(reader["VentaOriginalId"]),
                                    Fecha = Convert.ToDateTime(reader["Fecha"]),
                                    TipoDevolucion = reader["TipoDevolucion"]?.ToString() ?? string.Empty,
                                    TipoReembolso = reader["TipoReembolso"]?.ToString() ?? string.Empty,
                                    MetodoPagoOriginal = reader["MetodoPagoOriginal"]?.ToString() ?? string.Empty,
                                    MontoTotal = Convert.ToDecimal(reader["MontoTotal"]),
                                    MontoDevuelto = Convert.ToDecimal(reader["MontoDevuelto"]),
                                    Motivo = reader["Motivo"]?.ToString(),
                                    Estado = reader["Estado"]?.ToString() ?? string.Empty,
                                    FechaVentaOriginal = Convert.ToDateTime(reader["FechaVentaOriginal"]),
                                    CantidadProductosDevueltos = Convert.ToInt32(reader["CantidadProductosDevueltos"]),
                                    ClienteCredito = reader["ClienteCredito"]?.ToString(),
                                    IdentificacionCliente = reader["IdentificacionCliente"]?.ToString()
                                });
                            }
                            
                            return Ok(devoluciones);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("ValidarVentaParaDevolucion/{ventaId}")]
        public IActionResult ValidarVentaParaDevolucion(int ventaId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    
                    // Verificar si la venta existe y obtener detalles
                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT 
                            v.Id,
                            v.Fecha,
                            v.MetodoPago,
                            SUM(dv.Cantidad * dv.PrecioUnitario) AS Total,
                            COUNT(dv.Id) AS CantidadProductos,
                            CASE WHEN EXISTS(SELECT 1 FROM Devolucion WHERE VentaOriginalId = v.Id AND Estado = 'Procesada') 
                                 THEN 1 ELSE 0 END AS YaDevuelta
                        FROM Venta v
                        INNER JOIN DetalleVenta dv ON v.Id = dv.VentaId
                        WHERE v.Id = @VentaId
                        GROUP BY v.Id, v.Fecha, v.MetodoPago", conn))
                    {
                        cmd.Parameters.AddWithValue("@VentaId", ventaId);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new {
                                    ventaId = Convert.ToInt32(reader["Id"]),
                                    fecha = Convert.ToDateTime(reader["Fecha"]),
                                    metodoPago = reader["MetodoPago"]?.ToString(),
                                    total = Convert.ToDecimal(reader["Total"]),
                                    cantidadProductos = Convert.ToInt32(reader["CantidadProductos"]),
                                    yaDevuelta = Convert.ToBoolean(reader["YaDevuelta"]),
                                    valida = !Convert.ToBoolean(reader["YaDevuelta"])
                                });
                            }
                            else
                            {
                                return NotFound(new { error = "La venta especificada no existe" });
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
