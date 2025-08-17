using AntojeriaTica_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AntojeriaTica_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReporteController : ControllerBase
    {
        private readonly string _connectionString;

        public ReporteController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet("ventas-anuales/{anio}")]
        public IActionResult GetVentasAnuales(int anio)
        {
            List<ReporteVentasAnualesResponse> lista = new();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_ReporteVentasAnual", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Anio", anio);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new ReporteVentasAnualesResponse
                        {
                            Mes = Convert.ToInt32(dr["Mes"]),
                            NombreMes = dr["NombreMes"].ToString(),
                            TotalVentas = Convert.ToDecimal(dr["TotalVentas"]),
                            CantidadVentas = Convert.ToInt32(dr["CantidadVentas"])
                        });
                    }
                }
            }

            return Ok(lista);
        }


        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            DashboardModel dashboard = new DashboardModel();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_DashboardVentas", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        dashboard.TotalVentasHoy = Convert.ToDecimal(dr["TotalVentasHoy"]);
                        dashboard.CantidadPedidosHoy = Convert.ToInt32(dr["CantidadPedidosHoy"]);
                        dashboard.TendenciaSemana = Convert.ToDecimal(dr["TendenciaSemana"]);
                    }

                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            dashboard.UltimosDias.Add(dr["Dia"].ToString());
                            dashboard.VentasUltimosDias.Add(Convert.ToDecimal(dr["Total"]));

                        }
                    }
                }
            }

            return Ok(dashboard);
        }
    }
}
