using AntojeriaTica_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace AntojeriaTica_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistorialController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HistorialController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("FiltrarHistorialVentas")]
        public IActionResult FiltrarHistorialVentas([FromBody] HistorialFiltroRequest filtro)
        {
            var resultados = new List<HistorialVenta>();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("sp_ListarHistorialVenta", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@FechaInicio", (object)filtro.FechaInicio ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaFin", (object)filtro.FechaFin ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TipoOperacion", (object)filtro.TipoOperacion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Usuario", (object)filtro.Usuario ?? DBNull.Value);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(new HistorialVenta
                            {
                                IdHistorial = reader.GetInt32(0),
                                IdVenta = reader.GetInt32(1),
                                FechaModificacion = reader.GetDateTime(2),
                                TipoOperacion = reader.GetString(3),
                                UsuarioModificador = reader.GetString(4),
                                DatosAntes = reader.IsDBNull(5) ? null : reader.GetString(5),
                                DatosDespues = reader.IsDBNull(6) ? null : reader.GetString(6)
                            });
                        }
                    }
                }
            }

            return Ok(resultados);
        }

        [HttpPost("FiltrarHistorialDetalleVentas")]
        public IActionResult FiltrarHistorialDetalleVentas([FromBody] HistorialFiltroRequest filtro)
        {
            var resultados = new List<HistorialDetalleVenta>();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();

                var query = "SELECT * FROM HistorialDetalleVenta WHERE 1 = 1";

                if (filtro.FechaInicio.HasValue)
                    query += " AND FechaModificacion >= @FechaInicio";

                if (filtro.FechaFin.HasValue)
                    query += " AND FechaModificacion <= @FechaFin";

                if (!string.IsNullOrEmpty(filtro.TipoOperacion))
                    query += " AND TipoOperacion = @TipoOperacion";

                if (!string.IsNullOrEmpty(filtro.Usuario))
                    query += " AND UsuarioModificador = @Usuario";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (filtro.FechaInicio.HasValue)
                        cmd.Parameters.AddWithValue("@FechaInicio", filtro.FechaInicio.Value);

                    if (filtro.FechaFin.HasValue)
                        cmd.Parameters.AddWithValue("@FechaFin", filtro.FechaFin.Value);

                    if (!string.IsNullOrEmpty(filtro.TipoOperacion))
                        cmd.Parameters.AddWithValue("@TipoOperacion", filtro.TipoOperacion);

                    if (!string.IsNullOrEmpty(filtro.Usuario))
                        cmd.Parameters.AddWithValue("@Usuario", filtro.Usuario);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(new HistorialDetalleVenta
                            {
                                IdHistorial = reader.GetInt32(0),
                                IdDetalleVenta = reader.GetInt32(1),
                                FechaModificacion = reader.GetDateTime(2),
                                TipoOperacion = reader.GetString(3),
                                UsuarioModificador = reader.GetString(4),
                                DatosAntes = reader.IsDBNull(5) ? null : reader.GetString(5),
                                DatosDespues = reader.IsDBNull(6) ? null : reader.GetString(6)
                            });
                        }
                    }
                }
            }

            return Ok(resultados);
        }
    }

}