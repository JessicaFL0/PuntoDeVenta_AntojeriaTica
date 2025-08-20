using AntojeriaTica_Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;

namespace AntojeriaTica_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetodoPagoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public MetodoPagoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            var lista = new List<MetodoPago>();
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id, Nombre, Activo FROM MetodoPago", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new MetodoPago
                    {
                        IdMetodoPago = Convert.ToInt32(reader["Id"]),
                        Nombre = reader["Nombre"].ToString(),
                        EstaActivo = Convert.ToBoolean(reader["Activo"])
                    });
                }
            }
            return Ok(lista);
        }

        [HttpPost("Agregar")]
        public IActionResult Agregar([FromBody] MetodoPago metodo)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO MetodoPago (Nombre, Activo) VALUES (@Nombre, 1)", conn);
                cmd.Parameters.AddWithValue("@Nombre", metodo.Nombre);
                cmd.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Método de pago agregado correctamente" });
        }

        [HttpPut("CambiarEstado/{id}")]
        public IActionResult CambiarEstado(int id)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE MetodoPago SET Activo = 
                    CASE WHEN Activo = 1 THEN 0 ELSE 1 END 
                    WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Estado cambiado correctamente" });
        }

        [HttpGet("Historial/{id}")]
        public IActionResult Historial(int id)
        {
            var historial = new List<HistorialMetodoPago>();
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Id, MetodoPagoId, FechaPago, Monto FROM HistorialMetodoPago WHERE MetodoPagoId = @Id ORDER BY FechaPago DESC", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    historial.Add(new HistorialMetodoPago
                    {
                        IdHistorial = Convert.ToInt32(reader["Id"]),
                        IdMetodoPago = Convert.ToInt32(reader["MetodoPagoId"]),
                        FechaModificacion = Convert.ToDateTime(reader["FechaPago"]),
                        Accion = "Pago",
                        UsuarioModificador = "Sistema"
                    });
                }
            }
            return Ok(historial);
        }
    }
}
