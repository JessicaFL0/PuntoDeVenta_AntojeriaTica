using AntojeriaTica_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AntojeriaTica_Api.Controllers
{
    [Route("api/Impuestos")]
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
            var lista = new List<Impuesto>();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT Id, Nombre, Porcentaje, Activo FROM Impuesto", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var nombre = reader["Nombre"].ToString() ?? string.Empty;
                    lista.Add(new Impuesto
                    {
                        IdImpuesto = Convert.ToInt32(reader["Id"]),
                        Nombre = nombre,
                        Tipo = nombre.ToUpper() == "IVA" ? "IVA" : "ISC",
                        Porcentaje = Convert.ToDecimal(reader["Porcentaje"]),
                        AplicaEnRestaurante = false,
                        EsExonerado = nombre.ToUpper() == "EXENTO",
                        Estado = Convert.ToBoolean(reader["Activo"])
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
                using var cmd = new SqlCommand(@"INSERT INTO Impuesto (Nombre, Porcentaje, Activo) 
                                                VALUES (@Nombre, @Porcentaje, 1)", conn);
                cmd.Parameters.AddWithValue("@Nombre", (object?)imp.Nombre ?? string.Empty);
                cmd.Parameters.AddWithValue("@Porcentaje", imp.Porcentaje);
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
                using var cmd = new SqlCommand("UPDATE Impuesto SET Activo = CASE WHEN Activo = 1 THEN 0 ELSE 1 END WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }

            return Ok(new { mensaje = "Estado cambiado correctamente" });
        }
    }
}
