using AntojeriaTica_Api.Models;
using AntojeriaTica_Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace AntojeriaTica_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacturaElectronicaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IFacturaElectronicaService _facturaService;

        public FacturaElectronicaController(IConfiguration configuration, IFacturaElectronicaService facturaService)
        {
            _configuration = configuration;
            _facturaService = facturaService;
        }

        // Escenario 1: Generar factura electrónica para una venta
        [HttpPost("GenerarFactura")]
    public IActionResult GenerarFacturaElectronica([FromBody] GenerarFacturaRequest request)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("GenerarFacturaElectronica", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VentaId", request.VentaId);
                        cmd.Parameters.AddWithValue("@ClienteNombre", request.ClienteNombre);
                        cmd.Parameters.AddWithValue("@ClienteEmail", request.ClienteEmail ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ClienteTelefono", request.ClienteTelefono ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@IdentificacionCliente", request.ClienteIdentificacion ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreadoPor", "Sistema");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var response = new FacturaResponse
                                {
                                    IdFactura = Convert.ToInt32(reader["IdFactura"]),
                                    NumeroFactura = reader["NumeroFactura"]?.ToString() ?? string.Empty,
                                    ClaveNumerica = reader["ClaveNumerica"]?.ToString() ?? string.Empty,
                                    FechaEmision = Convert.ToDateTime(reader["FechaEmision"]),
                                    SubTotal = Convert.ToDecimal(reader["SubTotal"]),
                                    MontoImpuesto = Convert.ToDecimal(reader["MontoImpuesto"]),
                                    MontoTotal = Convert.ToDecimal(reader["MontoTotal"]),
                                    EstadoHacienda = reader["EstadoHacienda"]?.ToString() ?? string.Empty
                                };

                                // Cerrar reader antes de hacer otras operaciones
                                reader.Close();

                                // Simular envío a Hacienda
                                var estadoSimulado = SimularEnvioHacienda(response.ClaveNumerica ?? "");
                                response.EstadoHacienda = estadoSimulado;
                                
                                // Actualizar estado en base de datos
                                ActualizarEstadoHacienda(conn, response.IdFactura, estadoSimulado);

                                // Enviar email si se proporciona (asíncrono)
                                if (!string.IsNullOrEmpty(request.ClienteEmail))
                                {
                                    _ = Task.Run(async () => await _facturaService.EnviarEmailFacturaAsync(response.IdFactura, request.ClienteEmail));
                                }

                                return Ok(response);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al generar la factura: {ex.Message}");
            }

            return BadRequest("No se pudo generar la factura");
        }

        // Escenario 2: Buscar facturas electrónicas
        [HttpGet("BuscarFacturas")]
        public IActionResult BuscarFacturas(DateTime? fechaInicio = null, DateTime? fechaFin = null, 
            string? numeroFactura = null, string? clienteNombre = null, string? estadoHacienda = null)
        {
            try
            {
                var facturas = new List<FacturaElectronica>();

                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("BuscarFacturasElectronicas", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@NumeroFactura", numeroFactura ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ClienteNombre", clienteNombre ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@EstadoHacienda", estadoHacienda ?? (object)DBNull.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                facturas.Add(new FacturaElectronica
                                {
                                    Id = Convert.ToInt32(reader["IdFactura"]),
                                    NumeroFactura = reader["NumeroFactura"]?.ToString() ?? "",
                                    ClaveNumerica = reader["ClaveNumerica"]?.ToString() ?? "",
                                    NombreCliente = reader["ClienteNombre"]?.ToString() ?? "",
                                    CorreoCliente = reader["ClienteEmail"]?.ToString(),
                                    FechaGeneracion = Convert.ToDateTime(reader["FechaEmision"]),
                                    TotalComprobante = Convert.ToDecimal(reader["MontoTotal"]),
                                    EstadoHacienda = reader["EstadoHacienda"]?.ToString() ?? "",
                                    VentaId = Convert.ToInt32(reader["VentaId"])
                                });
                            }
                        }
                    }
                }

                return Ok(facturas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Escenario 3: Obtener detalle de factura
        [HttpGet("DetalleFactura/{id}")]
        public IActionResult DetalleFactura(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("ObtenerDetalleFacturaElectronica", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IdFactura", id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var detalle = new DetalleFacturaElectronica
                                {
                                    IdFactura = Convert.ToInt32(reader["IdFactura"]),
                                    NumeroFactura = reader["NumeroFactura"]?.ToString() ?? string.Empty,
                                    ClaveNumerica = reader["ClaveNumerica"]?.ToString() ?? string.Empty,
                                    ClienteNombre = reader["ClienteNombre"]?.ToString() ?? string.Empty,
                                    ClienteEmail = reader["ClienteEmail"]?.ToString(),
                                    ClienteTelefono = reader["ClienteTelefono"]?.ToString(),
                                    ClienteIdentificacion = reader["ClienteIdentificacion"]?.ToString(),
                                    FechaEmision = Convert.ToDateTime(reader["FechaEmision"]),
                                    SubTotal = Convert.ToDecimal(reader["SubTotal"]),
                                    MontoImpuesto = Convert.ToDecimal(reader["MontoImpuesto"]),
                                    MontoTotal = Convert.ToDecimal(reader["MontoTotal"]),
                                    EstadoHacienda = reader["EstadoHacienda"]?.ToString() ?? string.Empty,
                                    MensajeHacienda = reader["MensajeHacienda"]?.ToString(),
                                    EmailEnviado = Convert.ToBoolean(reader["EmailEnviado"]),
                                    FechaEnvioEmail = reader["FechaEnvioEmail"] != DBNull.Value 
                                        ? Convert.ToDateTime(reader["FechaEnvioEmail"]) : (DateTime?)null,
                                    VentaId = Convert.ToInt32(reader["VentaId"]),
                                    FechaVenta = Convert.ToDateTime(reader["FechaVenta"])
                                };

                                // Obtener productos (segundo result set)
                                if (reader.NextResult())
                                {
                                    detalle.Productos = new List<DetalleProductoFactura>();
                                    while (reader.Read())
                                    {
                                        detalle.Productos.Add(new DetalleProductoFactura
                                        {
                                            IdProducto = Convert.ToInt32(reader["IdProducto"]),
                                            Codigo = reader["Codigo"]?.ToString(),
                                            Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                                            Descripcion = reader["Descripcion"]?.ToString(),
                                            Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                            PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"]),
                                            SubTotal = Convert.ToDecimal(reader["SubTotal"]),
                                            PorcentajeImpuesto = Convert.ToDecimal(reader["PorcentajeImpuesto"]),
                                            MontoImpuesto = Convert.ToDecimal(reader["MontoImpuesto"])
                                        });
                                    }
                                }

                                // Obtener historial (tercer result set)
                                if (reader.NextResult())
                                {
                                    detalle.HistorialEnvios = new List<HistorialEnvioFactura>();
                                    while (reader.Read())
                                    {
                                        detalle.HistorialEnvios.Add(new HistorialEnvioFactura
                                        {
                                            IdHistorial = Convert.ToInt32(reader["IdHistorial"]),
                                            TipoEnvio = reader["TipoEnvio"]?.ToString() ?? string.Empty,
                                            Destinatario = reader["Destinatario"]?.ToString(),
                                            EstadoEnvio = reader["EstadoEnvio"]?.ToString() ?? string.Empty,
                                            MensajeRespuesta = reader["MensajeRespuesta"]?.ToString(),
                                            FechaEnvio = Convert.ToDateTime(reader["FechaEnvio"])
                                        });
                                    }
                                }

                                return Ok(detalle);
                            }
                            else
                            {
                                return NotFound(new { error = "Factura no encontrada" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Escenario 4: Reenviar factura por email
        [HttpPost("ReenviarFactura/{id}")]
        public async Task<IActionResult> ReenviarFactura(int id, [FromBody] ReenvioFacturaRequest request)
        {
            try
            {
                var resultado = await _facturaService.EnviarEmailFacturaAsync(id, request.Email);
                
                if (resultado)
                {
                    return Ok(new { message = "Email reenviado exitosamente" });
                }
                else
                {
                    return BadRequest(new { error = "No se pudo reenviar el email" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // Escenario 5: Descargar PDF de factura
        [HttpGet("DescargarPDF/{id}")]
        public IActionResult DescargarPDF(int id)
        {
            try
            {
                var pdfContent = _facturaService.GenerarPDFFactura(id);
                return File(pdfContent, "application/pdf", $"Factura-{id}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #region Métodos Privados

        private string SimularEnvioHacienda(string claveNumerica)
        {
            // Simulación simple: 90% de éxito
            var random = new Random();
            return random.NextDouble() > 0.1 ? "Aceptado" : "Rechazado";
        }

        private void ActualizarEstadoHacienda(SqlConnection conn, int idFactura, string estado)
        {
            using (SqlCommand cmd = new SqlCommand("ActualizarEstadoFacturaHacienda", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdFactura", idFactura);
                cmd.Parameters.AddWithValue("@EstadoHacienda", estado);
                cmd.Parameters.AddWithValue("@MensajeHacienda", $"Procesado automáticamente - {DateTime.Now}");
                cmd.Parameters.AddWithValue("@XMLGenerado", "<xml>Simulado</xml>");
                cmd.ExecuteNonQuery();
            }
        }

        private bool EnviarEmailFactura(SqlConnection conn, int idFactura, string email)
        {
            try
            {
                // Simular envío de email
                using (SqlCommand cmd = new SqlCommand("RegistrarEnvioFactura", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdFactura", idFactura);
                    cmd.Parameters.AddWithValue("@TipoEnvio", "Email");
                    cmd.Parameters.AddWithValue("@Destinatario", email);
                    cmd.Parameters.AddWithValue("@EstadoEnvio", "Enviado");
                    cmd.Parameters.AddWithValue("@MensajeRespuesta", "Email enviado exitosamente");
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private byte[] GenerarPDFSimulado(int idFactura)
        {
            // Contenido PDF simulado
            var content = $"Factura Electrónica #{idFactura}\nGenerada el: {DateTime.Now}\nEste es un PDF de prueba.";
            return Encoding.UTF8.GetBytes(content);
        }

        #endregion
    }
}
