using AntojeriaTica_Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace AntojeriaTica_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimientoDiarioController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public MovimientoDiarioController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] MovimientoDiario movimiento)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("InsertarMovimientoDiario", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TipoMovimiento", movimiento.TipoMovimiento);
                cmd.Parameters.AddWithValue("@Categoria", movimiento.Categoria);
                cmd.Parameters.AddWithValue("@Monto", movimiento.Monto);
                cmd.Parameters.AddWithValue("@Descripcion", movimiento.Descripcion ?? "");
                cmd.Parameters.AddWithValue("@IdUsuario", movimiento.IdUsuario);

                con.Open();
                cmd.ExecuteNonQuery();
                return Ok(new { mensaje = "Movimiento registrado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al registrar el movimiento", error = ex.Message });
            }
        }

        [HttpGet("listar")]
        public IActionResult Listar()
        {
            List<MovimientoDiario> lista = new();

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("EXEC sp_ListarMovimientosConNombre", connection);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new MovimientoDiario
                    {
                        IdMovimiento = (int)reader["IdMovimiento"],
                        Fecha = (DateTime)reader["FechaHora"], // <- Corregido
                        TipoMovimiento = reader["TipoMovimiento"].ToString(),
                        Categoria = reader["Categoria"].ToString(),
                        Monto = Convert.ToDecimal(reader["Monto"]),
                        Descripcion = reader["Descripcion"].ToString(),
                        IdUsuario = (int)reader["IdUsuario"],
                        NombreUsuario = reader["NombreUsuario"].ToString() // <- este lo devuelve el SP correctamente
                    });
                }
            }

            return Ok(lista);
        }


        [HttpPost("actualizar")]
        public IActionResult Actualizar([FromBody] MovimientoDiario movimiento)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("ActualizarMovimientoDiario", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdMovimiento", movimiento.IdMovimiento);
                cmd.Parameters.AddWithValue("@TipoMovimiento", movimiento.TipoMovimiento);
                cmd.Parameters.AddWithValue("@Categoria", movimiento.Categoria);
                cmd.Parameters.AddWithValue("@Monto", movimiento.Monto);
                cmd.Parameters.AddWithValue("@Descripcion", movimiento.Descripcion ?? "");

                con.Open();
                cmd.ExecuteNonQuery();
                return Ok(new { mensaje = "Movimiento actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al actualizar el movimiento", error = ex.Message });
            }
        }

        [HttpPost("eliminar")]
        public IActionResult Eliminar([FromBody] int id)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("EliminarMovimientoDiario", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdMovimiento", id);

                con.Open();
                cmd.ExecuteNonQuery();
                return Ok(new { mensaje = "Movimiento eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al eliminar el movimiento", error = ex.Message });
            }
        }
    }
}
