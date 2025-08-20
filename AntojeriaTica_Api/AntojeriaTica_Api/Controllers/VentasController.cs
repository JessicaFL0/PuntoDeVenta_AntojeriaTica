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
    public class VentasController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public VentasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("RegistrarVenta")]
    public IActionResult RegistrarVenta([FromBody] Venta venta)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();

                    DataTable detalleVentaTable = new DataTable();
                    detalleVentaTable.Columns.Add("ProductoId", typeof(int));
                    detalleVentaTable.Columns.Add("Cantidad", typeof(int));
                    detalleVentaTable.Columns.Add("PrecioUnitario", typeof(decimal));

                    foreach (var detalle in venta.Detalles)
                    {
                        detalleVentaTable.Rows.Add(detalle.ProductoId, detalle.Cantidad, detalle.PrecioUnitario);
                    }

                    using (SqlCommand cmd = new SqlCommand("RegistrarVenta", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MetodoPago", venta.MetodoPago);
                        // PedidoId opcional para ventas basadas en pedidos
                        cmd.Parameters.AddWithValue("@PedidoId", (object?)venta.PedidoId ?? DBNull.Value);

                        SqlParameter tvpParam = cmd.Parameters.AddWithValue("@DetallesVenta", detalleVentaTable);
                        tvpParam.SqlDbType = SqlDbType.Structured;
                        tvpParam.TypeName = "TipoDetalleVenta";

                        cmd.ExecuteNonQuery();
                    }
                }

                return Ok(new { mensaje = "Venta registrada correctamente" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("BuscarVentas")]
        public IActionResult BuscarVentas([FromQuery] DateTime? fechaInicio = null, 
            [FromQuery] DateTime? fechaFin = null, 
            [FromQuery] string? metodoPago = null, 
            [FromQuery] int? ventaId = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("BuscarVentas", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.HasValue ? (object)fechaInicio.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin.HasValue ? (object)fechaFin.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MetodoPago", string.IsNullOrEmpty(metodoPago) ? (object)DBNull.Value : metodoPago);
                        cmd.Parameters.AddWithValue("@VentaId", ventaId.HasValue ? (object)ventaId.Value : DBNull.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var ventas = new List<VentaDetallada>();
                            
                            while (reader.Read())
                            {
                                ventas.Add(new VentaDetallada
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Fecha = Convert.ToDateTime(reader["Fecha"]),
                                    MetodoPago = reader["MetodoPago"]?.ToString() ?? string.Empty,
                                    Total = Convert.ToDecimal(reader["Total"]),
                                    CantidadProductos = Convert.ToInt32(reader["CantidadProductos"])
                                });
                            }
                            
                            return Ok(ventas);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Log más detallado del error
                return BadRequest(new { 
                    error = ex.Message, 
                    stackTrace = ex.StackTrace,
                    parameters = new {
                        fechaInicio,
                        fechaFin,
                        metodoPago,
                        ventaId
                    }
                });
            }
        }

        [HttpGet("DetalleVenta/{ventaId}")]
        public IActionResult ObtenerDetalleVenta(int ventaId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("ObtenerDetalleVenta", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VentaId", ventaId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var venta = new VentaCompleta();
                            var detalles = new List<DetalleVentaCompleto>();
                            
                            while (reader.Read())
                            {
                                if (venta.Id == 0)
                                {
                                    venta.Id = Convert.ToInt32(reader["VentaId"]);
                                    venta.Fecha = Convert.ToDateTime(reader["Fecha"]);
                                    venta.MetodoPago = reader["MetodoPago"]?.ToString() ?? string.Empty;
                                    venta.Total = Convert.ToDecimal(reader["TotalVenta"]);
                                }
                                
                                detalles.Add(new DetalleVentaCompleto
                                {
                                    ProductoId = Convert.ToInt32(reader["ProductoId"]),
                                    ProductoNombre = reader["ProductoNombre"]?.ToString() ?? string.Empty,
                                    ProductoCodigo = reader["ProductoCodigo"]?.ToString() ?? string.Empty,
                                    Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                    PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"]),
                                    Subtotal = Convert.ToDecimal(reader["Subtotal"])
                                });
                            }
                            
                            venta.Detalles = detalles;
                            return Ok(venta);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("ReporteVentasDia")]
        public IActionResult ObtenerReporteVentasDia([FromQuery] DateTime fecha)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("ReporteVentasDia", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Fecha", fecha.Date);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var reporte = new ReporteVentasDia
                            {
                                Fecha = fecha.Date,
                                VentasPorMetodo = new List<VentasPorMetodo>(),
                                ProductosVendidos = new List<ProductoVendido>()
                            };
                            
                            while (reader.Read())
                            {
                                reporte.TotalVentas = Convert.ToInt32(reader["TotalVentas"]);
                                reporte.MontoTotal = Convert.ToDecimal(reader["MontoTotal"]);
                            }
                            
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    reporte.VentasPorMetodo.Add(new VentasPorMetodo
                                    {
                                        MetodoPago = reader["MetodoPago"]?.ToString() ?? string.Empty,
                                        CantidadVentas = Convert.ToInt32(reader["CantidadVentas"]),
                                        MontoTotal = Convert.ToDecimal(reader["MontoTotal"])
                                    });
                                }
                            }
                            
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    reporte.ProductosVendidos.Add(new ProductoVendido
                                    {
                                        ProductoCodigo = reader["ProductoCodigo"]?.ToString() ?? string.Empty,
                                        ProductoNombre = reader["ProductoNombre"]?.ToString() ?? string.Empty,
                                        CantidadVendida = Convert.ToInt32(reader["CantidadVendida"]),
                                        MontoTotal = Convert.ToDecimal(reader["MontoTotal"])
                                    });
                                }
                            }
                            
                            return Ok(reporte);
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
