using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

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
                // Log del error específico de SQL
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error de base de datos",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                // Log del error general
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
    }
}
