using AntojeriaTica_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AntojeriaTica_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public VentasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("RegistrarVenta")]
        public IActionResult RegistrarVenta([FromBody] Venta venta)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();

                    DataTable detalleVentaTable = new DataTable();
                    detalleVentaTable.Columns.Add("ProductoId", typeof(int));
                    detalleVentaTable.Columns.Add("Cantidad", typeof(int));
                    detalleVentaTable.Columns.Add("PrecioUnitario", typeof(decimal));

                    foreach (var detalle in venta.Detalles)
                    {
                        detalleVentaTable.Rows.Add(detalle.ProductoId, detalle.Cantidad, detalle.PrecioUnitario);
                    }

                    using (SqlCommand cmd = new SqlCommand("RegistrarVenta", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MetodoPago", venta.MetodoPago);

                        SqlParameter tvpParam = cmd.Parameters.AddWithValue("@DetallesVenta", detalleVentaTable);
                        tvpParam.SqlDbType = SqlDbType.Structured;
                        tvpParam.TypeName = "TipoDetalleVenta";

                        cmd.ExecuteNonQuery();
                    }
                }

                return Ok(new { mensaje = "Venta registrada correctamente" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
