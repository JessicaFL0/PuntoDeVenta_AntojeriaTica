using AntojeriaTica_Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace AntojeriaTica_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CierreCajaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public CierreCajaController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet("obtenerTotalesHoy")]
        public IActionResult ObtenerTotalesHoy()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand("sp_CierreCajaDiario", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                con.Open();
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var result = new
                    {
                        TotalIngresos = Convert.ToDecimal(reader["TotalIngresos"]),
                        TotalEgresos = Convert.ToDecimal(reader["TotalEgresos"])
                    };
                    return Ok(result);
                }

                return BadRequest(new { mensaje = "No se pudo obtener los totales" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al obtener los totales", error = ex.Message });
            }
        }
        [HttpGet("listar")]
        public IActionResult Listar()
        {
            List<CierreCaja> lista = new();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_ListarCierresDeCaja", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new CierreCaja
                            {
                                IdMovimiento = Convert.ToInt32(reader["IdMovimiento"]),
                                Fecha = Convert.ToDateTime(reader["FechaHora"]),
                                TotalIngresos = Convert.ToDecimal(reader["TotalIngresos"]),
                                TotalEgresos = Convert.ToDecimal(reader["TotalEgresos"]),
                                MontoFisico = Convert.ToDecimal(reader["MontoFisico"]),
                                NotaJustificativa = reader["NotaJustificativa"]?.ToString(),
                                NombreUsuario = reader["NombreUsuario"].ToString()
                            });
                        }
                    }
                }
            }

            return Ok(lista);
        }

    }
}
