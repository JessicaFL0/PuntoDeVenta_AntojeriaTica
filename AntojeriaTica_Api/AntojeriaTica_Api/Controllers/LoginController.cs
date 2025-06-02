using AntojeriaTica_Web.Models;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AntojeriaTica_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        [Route("RegistrarCuenta")]
        public IActionResult RegistrarCuenta(UsuarioModel model)
        {
            using (var context = new SqlConnection("Server=ZU;Database=AntojeriaTicaDB;Trusted_Connection=True;TrustServerCertificate=True"))
            {
                using (SqlCommand comando = new SqlCommand ("sp_InsertarUsuario", context))
                {
                    comando.CommandType = System.Data.CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@Cedula", model.Cedula);
                    comando.Parameters.AddWithValue("@ContrasenaHash", model.ContrasenaHash);
                    comando.Parameters.AddWithValue("@NombreCompleto", model.NombreCompleto);
                    comando.Parameters.AddWithValue("@Correo", model.Correo);

                    context.Open();
                    comando.ExecuteNonQuery();
                }
                    //var result = context.Execute("sp_InsertarUsuario",
                    //    new { model.Cedula, model.ContrasenaHash, model.NombreCompleto, model.Correo });
            }

            return Ok();
        }

    }
}
