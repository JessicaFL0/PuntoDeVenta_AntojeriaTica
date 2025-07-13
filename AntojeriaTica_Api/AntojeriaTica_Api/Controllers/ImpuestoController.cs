using AntojeriaTica_Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace AntojeriaTica_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImpuestoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ImpuestoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            List<Impuesto> lista = new List<Impuesto>();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Impuesto", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Impuesto
                    {
                        IdImpuesto = Convert.ToInt32(reader["IdImpuesto"]),
                        Nombre = reader["Nombre"].ToString(),
                        Tipo = reader["Tipo"].ToString(),
                        Porcentaje = Convert.ToDecimal(reader["Porcentaje"]),
                        AplicaEnRestaurante = Convert.ToBoolean(reader["AplicaEnRestaurante"]),
                        EsExonerado = Convert.ToBoolean(reader["EsExonerado"]),
                        Estado = Convert.ToBoolean(reader["Estado"])
                    });
                }
            }

            return Ok(lista);
        }

        [HttpPost("Agregar")]
        public IActionResult Agregar([FromBody] Impuesto imp)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                var cmd = new SqlCommand(@"INSERT INTO Impuesto (Nombre, Tipo, Porcentaje, AplicaEnRestaurante, EsExonerado, Estado) 
                                           VALUES (@Nombre, @Tipo, @Porcentaje, @AplicaEnRestaurante, @EsExonerado, 1)", conn);

                cmd.Parameters.AddWithValue("@Nombre", imp.Nombre);
                cmd.Parameters.AddWithValue("@Tipo", imp.Tipo);
                cmd.Parameters.AddWithValue("@Porcentaje", imp.Porcentaje);
                cmd.Parameters.AddWithValue("@AplicaEnRestaurante", imp.AplicaEnRestaurante);
                cmd.Parameters.AddWithValue("@EsExonerado", imp.EsExonerado);

                cmd.ExecuteNonQuery();
            }

            return Ok(new { mensaje = "Impuesto agregado correctamente" });
        }

        [HttpPut("CambiarEstado/{id}")]
        public IActionResult CambiarEstado(int id)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE Impuesto SET Estado = CASE WHEN Estado = 1 THEN 0 ELSE 1 END WHERE IdImpuesto = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }

            return Ok(new { mensaje = "Estado cambiado correctamente" });
        }
    }
}
