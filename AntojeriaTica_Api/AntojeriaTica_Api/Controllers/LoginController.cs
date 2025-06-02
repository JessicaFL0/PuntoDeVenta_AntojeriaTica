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
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("RegistrarCuenta")]
        public IActionResult RegistrarCuenta(UsuarioModel model)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                using (var context = new SqlConnection(connectionString))
                {
                    using (SqlCommand comando = new SqlCommand("sp_InsertarUsuario", context))
                    {
                        comando.CommandType = System.Data.CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@NombreCompleto", model.NombreCompleto ?? "");
                        comando.Parameters.AddWithValue("@Correo", model.Correo ?? "");
                        comando.Parameters.AddWithValue("@Cedula", model.Cedula ?? "");
                        comando.Parameters.AddWithValue("@ContrasenaHash", model.ContrasenaHash ?? "");
                        comando.Parameters.AddWithValue("@Estado", model.Estado ?? "Activo");
                        comando.Parameters.AddWithValue("@IdRol", model.IdRol ?? 1);

                        context.Open();
                        var result = comando.ExecuteScalar();
                        
                        return Ok(new { 
                            success = true, 
                            message = "Usuario registrado exitosamente",
                            idUsuario = result
                        });
                    }
                }
            }
            catch (SqlException ex)
            {
                // Log del error específico de SQL
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error de base de datos", 
                    error = ex.Message 
                });
            }
            catch (Exception ex)
            {
                // Log del error general
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error interno del servidor", 
                    error = ex.Message 
                });
            }
        }
    }
}
