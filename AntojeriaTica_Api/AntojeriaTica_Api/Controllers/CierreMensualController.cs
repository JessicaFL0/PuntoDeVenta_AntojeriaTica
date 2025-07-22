using AntojeriaTica_Api.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace AntojeriaTica_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CierreMensualController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public CierreMensualController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet("listar")]
        public IActionResult Listar()
        {
            var lista = new List<CierreFinancieroMensual>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_ListarCierresFinancieros", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new CierreFinancieroMensual
                {
                    Mes = Convert.ToInt32(reader["Mes"]),
                    Anio = Convert.ToInt32(reader["Anio"]),
                    TotalIngresos = Convert.ToDecimal(reader["TotalIngresos"]),
                    TotalEgresos = Convert.ToDecimal(reader["TotalEgresos"]),
                    UtilidadNeta = Convert.ToDecimal(reader["UtilidadNeta"]),
                    FechaGeneracion = Convert.ToDateTime(reader["FechaGeneracion"]),
                    GeneradoPor = reader["GeneradoPor"].ToString(),
                    ComentarioJustificativo = reader["ComentarioJustificativo"]?.ToString()
                });
            }

            return Ok(lista);
        }

        [HttpGet("vista-previa")]
        public IActionResult ObtenerVistaPrevia([FromQuery] int mes, [FromQuery] int anio)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_CierreFinancieroMensual", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Mes", mes);
            cmd.Parameters.AddWithValue("@Anio", anio);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                var resultado = new CierreFinancieroMensual
                {
                    Mes = mes,
                    Anio = anio,
                    TotalIngresos = Convert.ToDecimal(reader["TotalIngresos"]),
                    TotalEgresos = Convert.ToDecimal(reader["TotalEgresos"]),
                    UtilidadNeta = Convert.ToDecimal(reader["UtilidadNeta"]),
                    FechaGeneracion = DateTime.Now,
                    GeneradoPor = "",
                    ComentarioJustificativo = null
                };
                return Ok(resultado);
            }

            return NotFound("No hay datos para el mes y año indicados.");
        }

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] RegistrarCierreMensualDto cierre)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_RegistrarCierreMensual", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Mes", cierre.Mes);
                cmd.Parameters.AddWithValue("@Anio", cierre.Anio);
                cmd.Parameters.AddWithValue("@Usuario", cierre.Usuario);
                cmd.Parameters.AddWithValue("@Comentario", (object?)cierre.Comentario ?? DBNull.Value);

                // Parámetro de retorno
                var returnParam = new SqlParameter("@ReturnVal", SqlDbType.Int)
                {
                    Direction = ParameterDirection.ReturnValue
                };
                cmd.Parameters.Add(returnParam);

                conn.Open();
                cmd.ExecuteNonQuery();

                int resultado = (int)returnParam.Value;

                if (resultado == 1)
                {
                    return BadRequest(new { mensaje = "Ya existe un cierre para ese mes y año." });
                }

                return Ok(new { mensaje = "Cierre mensual registrado correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al registrar cierre mensual", error = ex.Message });
            }
        }
        [HttpDelete("eliminar")]
        public IActionResult Eliminar([FromQuery] int mes, [FromQuery] int anio)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_EliminarCierreMensual", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Mes", mes);
                cmd.Parameters.AddWithValue("@Anio", anio);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                    return NotFound(new { mensaje = "No se encontró el cierre a eliminar." });

                return Ok(new { mensaje = "Cierre eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al eliminar el cierre.", error = ex.Message });
            }
        }
        [HttpPut("actualizarComentario")]
        public IActionResult ActualizarComentario([FromBody] ComentarioUpdateRequest request)
        {
            if (request == null || request.Mes == 0 || request.Anio == 0)
                return BadRequest(new { mensaje = "Datos inválidos" });

            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"UPDATE CierreFinancieroMensual 
                    SET ComentarioJustificativo = @Comentario 
                    WHERE Mes = @Mes AND Anio = @Anio";

                var affected = connection.Execute(sql, new
                {
                    Comentario = request.Comentario,
                    Mes = request.Mes,
                    Anio = request.Anio
                });

                if (affected > 0)
                    return Ok(new { mensaje = "Comentario actualizado correctamente." });
                else
                    return NotFound(new { mensaje = "No se encontró el cierre financiero." });
            }
        }


    }
}
