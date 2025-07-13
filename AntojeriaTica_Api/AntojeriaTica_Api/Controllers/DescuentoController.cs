using AntojeriaTica_Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace AntojeriaTica_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DescuentoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DescuentoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            var lista = new List<Descuento>();
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Descuento", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Descuento
                    {
                        IdDescuento = Convert.ToInt32(reader["IdDescuento"]),
                        Nombre = reader["Nombre"].ToString(),
                        Tipo = reader["Tipo"].ToString(),
                        Valor = Convert.ToDecimal(reader["Valor"]),
                        CodigoCupon = reader["CodigoCupon"]?.ToString(),
                        FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                        FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                        Estado = reader["Estado"].ToString(),
                        Restricciones = reader["Restricciones"]?.ToString()
                    });
                }
            }
            return Ok(lista);
        }

        [HttpPost("Agregar")]
        public IActionResult Agregar([FromBody] Descuento descuento)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Descuento 
                    (Nombre, Tipo, Valor, CodigoCupon, FechaInicio, FechaFin, Estado, Restricciones)
                    VALUES (@Nombre, @Tipo, @Valor, @CodigoCupon, @FechaInicio, @FechaFin, @Estado, @Restricciones)", conn);

                cmd.Parameters.AddWithValue("@Nombre", descuento.Nombre);
                cmd.Parameters.AddWithValue("@Tipo", descuento.Tipo);
                cmd.Parameters.AddWithValue("@Valor", descuento.Valor);
                cmd.Parameters.AddWithValue("@CodigoCupon", (object?)descuento.CodigoCupon ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaInicio", descuento.FechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", descuento.FechaFin);
                cmd.Parameters.AddWithValue("@Estado", descuento.Estado);
                cmd.Parameters.AddWithValue("@Restricciones", (object?)descuento.Restricciones ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }

            return Ok(new { mensaje = "Descuento registrado correctamente" });
        }

        [HttpPut("CambiarEstado/{id}")]
        public IActionResult CambiarEstado(int id)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Descuento 
                    SET Estado = 
                        CASE 
                            WHEN Estado = 'Activo' THEN 'Inactivo'
                            WHEN Estado = 'Inactivo' THEN 'Activo'
                            ELSE Estado 
                        END
                    WHERE IdDescuento = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }

            return Ok(new { mensaje = "Estado del descuento actualizado" });
        }

        [HttpGet("ValidarCupon/{codigo}")]
        public IActionResult ValidarCupon(string codigo)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP 1 *
                    FROM Descuento
                    WHERE Codigo = @Codigo
                    AND Estado = 'activo'
                    AND FechaInicio <= GETDATE()
                    AND FechaFin >= GETDATE()", conn);

                cmd.Parameters.AddWithValue("@Codigo", codigo);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var descuento = new Descuento
                    {
                        IdDescuento = Convert.ToInt32(reader["IdDescuento"]),
                        Nombre = reader["Nombre"].ToString(),
                        Tipo = reader["Tipo"].ToString(),
                        Valor = Convert.ToDecimal(reader["Valor"]),
                        CodigoCupon = reader["Codigo"].ToString(),
                        FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                        FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                        Estado = reader["Estado"].ToString(),
                        Restricciones = reader["Restricciones"].ToString()
                    };

                    return Ok(descuento);
                }
            }

            return NotFound(new { mensaje = "Cupón inválido o vencido" });
        }
    }
}