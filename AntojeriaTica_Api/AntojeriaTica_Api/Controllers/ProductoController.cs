using AntojeriaTica_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace AntojeriaTica_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProductoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("RegistrarProducto")]
        public IActionResult RegistrarProducto([FromBody] Producto model)
        {
            if (string.IsNullOrWhiteSpace(model.Codigo) || string.IsNullOrWhiteSpace(model.Nombre))
            {
                return BadRequest(new { success = false, message = "Código y nombre son obligatorios" });
            }

            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("sp_InsertarProducto", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Codigo", model.Codigo);
                cmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", model.Descripcion ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PrecioUnitario", model.PrecioUnitario);
                cmd.Parameters.AddWithValue("@Existencias", model.Existencias);

                connection.Open();
                var id = Convert.ToInt32(cmd.ExecuteScalar());
                model.IdProducto = id;

                return Ok(new { success = true, message = "Producto registrado", producto = model });
            }
            catch (SqlException ex) when (ex.Number == 50000)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno", error = ex.Message });
            }
        }

        [HttpPut("ActualizarProducto")]
        public IActionResult ActualizarProducto([FromBody] Producto model)
        {
            if (model.IdProducto == null)
                return BadRequest(new { success = false, message = "IdProducto es requerido" });
            if (string.IsNullOrWhiteSpace(model.Nombre))
                return BadRequest(new { success = false, message = "Nombre es obligatorio" });

            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                connection.Open();

                Producto? existing = null;
                using (var getCmd = new SqlCommand("sp_ObtenerProducto", connection) { CommandType = CommandType.StoredProcedure })
                {
                    getCmd.Parameters.AddWithValue("@IdProducto", model.IdProducto);
                    using var reader = getCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        existing = new Producto
                        {
                            IdProducto = (int)reader["IdProducto"],
                            Codigo = reader["Codigo"].ToString() ?? string.Empty,
                            Nombre = reader["Nombre"].ToString() ?? string.Empty,
                            Descripcion = reader["Descripcion"].ToString(),
                            PrecioUnitario = (decimal)reader["PrecioUnitario"],
                            Existencias = (int)reader["Existencias"]
                        };
                    }
                }

                if (existing == null)
                    return NotFound(new { success = false, message = "Producto no encontrado" });

                // Actualizar
                using (var updCmd = new SqlCommand("sp_ActualizarProducto", connection) { CommandType = CommandType.StoredProcedure })
                {
                    updCmd.Parameters.AddWithValue("@IdProducto", model.IdProducto);
                    updCmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                    updCmd.Parameters.AddWithValue("@Descripcion", model.Descripcion ?? (object)DBNull.Value);
                    updCmd.Parameters.AddWithValue("@PrecioUnitario", model.PrecioUnitario);
                    updCmd.Parameters.AddWithValue("@Existencias", model.Existencias);
                    updCmd.ExecuteNonQuery();
                }

                // Historial
                var cambios = $"Existencias: {existing.Existencias} -> {model.Existencias}, Precio: {existing.PrecioUnitario} -> {model.PrecioUnitario}";
                using (var histCmd = new SqlCommand("sp_InsertarProductoHistorial", connection) { CommandType = CommandType.StoredProcedure })
                {
                    histCmd.Parameters.AddWithValue("@IdProducto", model.IdProducto);
                    histCmd.Parameters.AddWithValue("@Usuario", User?.Identity?.Name ?? "sistema");
                    histCmd.Parameters.AddWithValue("@Cambio", cambios);
                    histCmd.ExecuteNonQuery();
                }

                return Ok(new { success = true, message = "Producto actualizado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno", error = ex.Message });
            }
        }

        [HttpGet("ListarProductos")]
        public IActionResult ListarProductos()
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("sp_ObtenerProductos", connection) { CommandType = CommandType.StoredProcedure };
                connection.Open();
                using var reader = cmd.ExecuteReader();
                var list = new List<Producto>();
                while (reader.Read())
                {
                    list.Add(new Producto
                    {
                        IdProducto = (int)reader["IdProducto"],
                        Codigo = reader["Codigo"].ToString() ?? string.Empty,
                        Nombre = reader["Nombre"].ToString() ?? string.Empty,
                        Descripcion = reader["Descripcion"].ToString(),
                        PrecioUnitario = (decimal)reader["PrecioUnitario"],
                        Existencias = (int)reader["Existencias"]
                    });
                }
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno", error = ex.Message });
            }
        }

        [HttpGet("Historial/{id}")]
        public IActionResult Historial(int id)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("sp_ObtenerHistorialProducto", connection) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@IdProducto", id);
                connection.Open();
                using var reader = cmd.ExecuteReader();
                var hist = new List<ProductoHistory>();
                while (reader.Read())
                {
                    hist.Add(new ProductoHistory
                    {
                        IdProducto = (int)reader["IdProducto"],
                        Fecha = (DateTime)reader["Fecha"],
                        Usuario = reader["Usuario"].ToString() ?? string.Empty,
                        Cambio = reader["Cambio"].ToString() ?? string.Empty
                    });
                }
                return Ok(hist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno", error = ex.Message });
            }
        }
        //ELIMINAR PRODUCTO
        [HttpDelete("{id}")]
        public IActionResult EliminarProducto(int id)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("sp_EliminarProducto", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@IdProducto", id);
                connection.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { success = true, message = "Producto eliminado correctamente" });
            }
            catch (SqlException ex) when (ex.Number == 50000)
            {
                
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno", error = ex.Message });
            }
        }

        // REGISTRAR MOVIMIENT
        [HttpPost("RegistrarMovimiento")]
        public IActionResult RegistrarMovimiento([FromBody] MovimientoInventario model)
        {
            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("sp_RegistrarMovimientoInventario", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@IdProducto", model.IdProducto);
                cmd.Parameters.AddWithValue("@TipoMovimiento", model.TipoMovimiento);
                cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);

                conn.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { success = true, message = "Movimiento registrado correctamente" });
            }
            catch (SqlException ex) when (ex.Number == 50000)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno", error = ex.Message });
            }
        }



    }


}


