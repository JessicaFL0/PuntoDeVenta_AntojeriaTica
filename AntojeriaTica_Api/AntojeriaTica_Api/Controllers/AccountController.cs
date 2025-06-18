using AntojeriaTica_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;


namespace AntojeriaTica_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
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

                        return Ok(new
                        {
                            success = true,
                            message = "Usuario registrado exitosamente",
                            idUsuario = result
                        });
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error de base de datos",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("GetUser/{idUsuario}")]
        public IActionResult GetUserById(int idUsuario)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var context = new SqlConnection(connectionString))
                {
                    using (SqlCommand comando = new SqlCommand("sp_ObtenerUsuario", context))
                    {
                        comando.CommandType = System.Data.CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@IdUsuario", idUsuario);

                        context.Open();
                        using (var reader = comando.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var usuario = new
                                {
                                    IdUsuario = reader["IdUsuario"],
                                    NombreCompleto = reader["NombreCompleto"].ToString(),
                                    Correo = reader["Correo"].ToString(),
                                    Cedula = reader["Cedula"].ToString(),
                                    Estado = reader["Estado"].ToString(),
                                    IdRol = reader["IdRol"],
                                    NombreRol = reader["NombreRol"].ToString()
                                };

                                return Ok(new
                                {
                                    success = true,
                                    message = "Usuario obtenido exitosamente",
                                    user = usuario
                                });
                            }
                            else
                            {
                                return NotFound(new
                                {
                                    success = false,
                                    message = "Usuario no encontrado"
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error de base de datos",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var context = new SqlConnection(connectionString))
                {
                    using (SqlCommand comando = new SqlCommand("sp_ObtenerUsuarios", context))
                    {
                        comando.CommandType = System.Data.CommandType.StoredProcedure;

                        context.Open();
                        using (var reader = comando.ExecuteReader())
                        {
                            var usuarios = new List<object>();

                            while (reader.Read())
                            {
                                var usuario = new
                                {
                                    IdUsuario = reader["IdUsuario"],
                                    NombreCompleto = reader["NombreCompleto"].ToString(),
                                    Correo = reader["Correo"].ToString(),
                                    Cedula = reader["Cedula"].ToString(),
                                    Estado = reader["Estado"].ToString(),
                                    IdRol = reader["IdRol"],
                                    NombreRol = reader["NombreRol"].ToString()
                                };
                                usuarios.Add(usuario);
                            }

                            return Ok(new
                            {
                                success = true,
                                message = "Usuarios obtenidos exitosamente",
                                users = usuarios,
                                count = usuarios.Count
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error de base de datos",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        [HttpDelete]
        [Route("DeleteUser/{idUsuario}")]
        public IActionResult DeleteUser(int idUsuario)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var context = new SqlConnection(connectionString))
                {
                    using (SqlCommand comando = new SqlCommand("sp_EliminarUsuario", context))
                    {
                        comando.CommandType = System.Data.CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@IdUsuario", idUsuario);

                        context.Open();
                        var rowsAffected = comando.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(new
                            {
                                success = true,
                                message = "Usuario eliminado exitosamente"
                            });
                        }
                        else
                        {
                            return NotFound(new
                            {
                                success = false,
                                message = "Usuario no encontrado"
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error de base de datos",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        [HttpPut]
        [Route("UpdateUser")]
        public IActionResult UpdateUser(UsuarioModel model)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var context = new SqlConnection(connectionString))
                {
                    using (SqlCommand comando = new SqlCommand("sp_ActualizarUsuario", context))
                    {
                        comando.CommandType = System.Data.CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@IdUsuario", model.IdUsuario);
                        comando.Parameters.AddWithValue("@NombreCompleto", model.NombreCompleto ?? "");
                        comando.Parameters.AddWithValue("@Correo", model.Correo ?? "");
                        comando.Parameters.AddWithValue("@Cedula", model.Cedula ?? "");
                        comando.Parameters.AddWithValue("@Estado", model.Estado ?? "Activo");
                        comando.Parameters.AddWithValue("@IdRol", model.IdRol ?? 1);

                        context.Open();
                        var rowsAffected = comando.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(new
                            {
                                success = true,
                                message = "Usuario actualizado exitosamente"
                            });
                        }
                        else
                        {
                            return NotFound(new
                            {
                                success = false,
                                message = "Usuario no encontrado"
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error de base de datos",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        // roles
        [HttpPost]
        [Route("RegistrarRol")]
        public IActionResult RegistrarRol(Rol model)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var context = new SqlConnection(connectionString))
                using (var comando = new SqlCommand("sp_InsertarRol", context))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@NombreRol", model.Nombre);
                    comando.Parameters.AddWithValue("@Descripcion", model.Descripcion ?? "");

                    context.Open();
                    comando.ExecuteNonQuery();

                    return Ok(new { success = true, message = "Rol registrado correctamente" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error al registrar el rol", error = ex.Message });
            }
        }

        [HttpGet("GetAllRoles")]
        public IActionResult GetAllRoles()
        {
            List<Rol> lista = new List<Rol>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                using (SqlCommand command = new SqlCommand("sp_ListarRoles", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Rol rol = new Rol
                            {
                                IdRol = Convert.ToInt32(reader["IdRol"]),
                                Nombre = reader["NombreRol"]?.ToString() ?? "",
                                Descripcion = reader["Descripcion"]?.ToString() ?? ""
                            };

                            lista.Add(rol);
                        }
                    }
                }

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener los roles", detalles = ex.Message });
            }
        }
        [HttpDelete("EliminarRol/{id}")]
        public IActionResult EliminarRol(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    using (SqlCommand checkCommand = new SqlCommand("SELECT COUNT(*) FROM Usuario WHERE IdRol = @IdRol", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@IdRol", id);
                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            return BadRequest("No se puede eliminar el rol porque está en uso por usuarios.");
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("sp_EliminarRol", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IdRol", id);
                        cmd.ExecuteNonQuery();
                    }

                    return Ok(new { mensaje = "Rol eliminado correctamente" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al eliminar el rol", detalle = ex.Message });
            }
        }
        //termina rol

    }
}
