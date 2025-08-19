using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using AntojeriaTica_Web.Filters;

namespace AntojeriaTica_Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IHttpClientFactory httpClient, ILogger<LoginController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        [HttpGet]
        public IActionResult RegistrarCuenta()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegistrarCuenta(UsuarioModel model)
        {
            using (var httpClient = new HttpClient())
            {
                var url = "http://localhost:5062/api/Account/RegistrarCuenta";

                var newUserPayload = new
                {
                    model.NombreCompleto,
                    model.Correo,
                    model.Cedula,
                    Contrasena = model.ContrasenaHash,
                    Estado = "Activo"
                };

                var result = httpClient.PostAsJsonAsync(url, newUserPayload).Result;

                if (result.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Cuenta creada correctamente";
                    return RedirectToAction("IniciarSesion", "Login");
                }
                else
                {
                    var errorContent = result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("API Response: " + errorContent);
                    ViewBag.Error = "Error al crear la cuenta. El servidor respondió con: " + errorContent;
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult IniciarSesion()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CerrarSesion()
        {
            try
            {
                HttpContext.Session.Clear();
                TempData["Mensaje"] = "Sesión cerrada correctamente";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión");
                TempData["Error"] = "No se pudo cerrar la sesión";
            }
            return RedirectToAction("IniciarSesion");
        }

        [HttpGet]
        public IActionResult Principal()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("IniciarSesion");
            }

            ViewBag.Username = HttpContext.Session.GetString("NombreCompleto") ?? "Usuario";
            ViewBag.Token = token;
            ViewBag.Role = HttpContext.Session.GetString("NombreRol") ?? string.Empty;

            return View();
        }

        [HttpGet]
    public IActionResult RecuperarContrasena()
        {
            return View();
        }

        [HttpPost]
    [HttpPost]
    public IActionResult RecuperarContrasena(UsuarioModel model)
        {
            using (var httpClient = new HttpClient())
            {
                var url = "http://localhost:5062/api/Login/RecuperarContrasenna";
                var result = httpClient.PostAsJsonAsync(url, model).Result;

                if (result.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Se ha enviado un enlace de recuperación a tu correo.";
                    return RedirectToAction("IniciarSesion");
                }
            }

            ViewBag.Error = "No se pudo enviar el enlace. Verifica el correo.";
            return View(model);
        }

        //RegistrarEmpleado
        [HttpGet]
        [AdminOnly] 
        public async Task<IActionResult> RegistrarEmpleado()
        {
            try
            {
                var roles = await CargarRoles();
                ViewBag.Roles = roles; 
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar roles para el formulario: " + ex.Message;
                _logger.LogError(ex, "Error al cargar roles en GET RegistrarEmpleado");
            }

            return View(); 
        }

        [HttpPost]
        [AdminOnly]
        public async Task<IActionResult> RegistrarEmpleado(UsuarioModel model)
        {
            if (!ModelState.IsValid)
            {
                var roles = await CargarRoles();
                ViewBag.Roles = roles;
                return View(model);
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var url = "https://localhost:7243/api/Account/RegistrarCuenta";

                    var payload = new
                    {
                        model.NombreCompleto,
                        model.Correo,
                        model.Cedula,
                        Contrasena = model.ContrasenaHash ?? "",
                        Estado = model.Estado ?? "Activo",
                        model.IdRol
                    };

                    var response = await httpClient.PostAsJsonAsync(url, payload);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Empleado registrado correctamente";
                        return RedirectToAction("ListarUsuarios");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ViewBag.Error = "Error al registrar empleado: " + errorContent;
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error de conexión: " + ex.Message;
            }

            var rolesFallback = await CargarRoles();
            ViewBag.Roles = rolesFallback;
            return View(model);
        }

        private async Task<List<Rol>> CargarRoles()
        {
            List<Rol> roles = new List<Rol>();

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var rolesUrl = "https://localhost:7243/api/Account/GetAllRoles";
                    var result = await httpClient.GetAsync(rolesUrl);

                    if (result.IsSuccessStatusCode)
                    {
                        var json = await result.Content.ReadAsStringAsync();
                        roles = JsonSerializer.Deserialize<List<Rol>>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<Rol>();
                    }
                    else
                    {
                        ViewBag.Error = "Error al cargar roles desde la API.";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error de conexión al cargar roles: " + ex.Message;
            }

            return roles;
        }


        //Listar Usuarios
        [HttpGet]
            [AdminOnly]
            public IActionResult ListarUsuarios()
            {
                var usuarios = new List<UsuarioModel>();

                using (var httpClient = new HttpClient())
                {
                    var url = "http://localhost:5062/api/Account/GetAllUsers";
                    var result = httpClient.GetAsync(url).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        var responseContent = result.Content.ReadAsStringAsync().Result;
                        Console.WriteLine("API Response: " + responseContent);

                        ViewBag.UsuariosJson = responseContent;
                    }
                    else
                    {
                        var errorContent = result.Content.ReadAsStringAsync().Result;
                        Console.WriteLine("API Error: " + errorContent);
                        ViewBag.Error = "Error al obtener los usuarios: " + errorContent;
                    }
                }

                return View(usuarios);
            }

        [HttpGet]
        [AdminOnly]
        public IActionResult ActualizarUsuario(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("ListarUsuarios");
            }

            var model = new UsuarioModel();
            List<Rol> roles = new List<Rol>();

            using (var httpClient = new HttpClient())
            {
                var userUrl = $"http://localhost:5062/api/Account/GetUser/{id}";
                var userResult = httpClient.GetAsync(userUrl).Result;

                if (userResult.IsSuccessStatusCode)
                {
                    var responseContent = userResult.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("API Response: " + responseContent);

                    try
                    {
                        var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<dynamic>(responseContent);

                        var userElement = ((System.Text.Json.JsonElement)jsonResponse).GetProperty("user");

                        model.IdUsuario = userElement.GetProperty("idUsuario").GetInt32();
                        model.NombreCompleto = userElement.GetProperty("nombreCompleto").GetString();
                        model.Correo = userElement.GetProperty("correo").GetString();
                        model.Cedula = userElement.GetProperty("cedula").GetString();
                        model.Estado = userElement.GetProperty("estado").GetString();
                        model.IdRol = userElement.GetProperty("idRol").GetInt32();

                        Console.WriteLine($"Usuario cargado: {model.NombreCompleto} - {model.Correo}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error al deserializar: " + ex.Message);
                        model.IdUsuario = id;
                    }
                    ViewBag.UsuarioJson = responseContent;
                }
                else
                {
                    var errorContent = userResult.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("API Error: " + errorContent);
                    ViewBag.Error = "Error al obtener los datos del usuario: " + errorContent;
                    return RedirectToAction("ListarUsuarios");
                }

                // Cargar lista de roles
                var rolesUrl = "http://localhost:5062/api/Account/GetAllRoles";
                var rolesResult = httpClient.GetAsync(rolesUrl).Result;

                if (rolesResult.IsSuccessStatusCode)
                {
                    var rolesJson = rolesResult.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("Roles API Response: " + rolesJson);

                    roles = JsonSerializer.Deserialize<List<Rol>>(rolesJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Rol>();

                    Console.WriteLine($"Roles cargados: {roles.Count}");
                }
                else
                {
                    Console.WriteLine("Error al cargar roles para el formulario");
                }
            }

            ViewBag.Roles = roles;
            return View(model);

        }
                [HttpPost]
                [AdminOnly]
                public IActionResult ActualizarUsuario(UsuarioModel model)
                {
                    using (var httpClient = new HttpClient())
                    {
                        var url = "http://localhost:5062/api/Account/UpdateUser";

                        var updateUserPayload = new
                        {
                            model.IdUsuario,
                            model.NombreCompleto,
                            model.Correo,
                            model.Cedula,
                            model.Estado,
                            model.IdRol
                        };

                        var result = httpClient.PutAsJsonAsync(url, updateUserPayload).Result;

                        if (result.IsSuccessStatusCode)
                        {
                            TempData["Mensaje"] = "Usuario actualizado correctamente";
                            return RedirectToAction("ListarUsuarios", "Login");
                        }
                        else
                        {
                            var errorContent = result.Content.ReadAsStringAsync().Result;
                            Console.WriteLine("API Response: " + errorContent);
                            ViewBag.Error = "Error al actualizar el usuario. El servidor respondió con: " + errorContent;

                            List<Rol> roles = new List<Rol>();
                            var rolesUrl = "http://localhost:5062/api/Account/GetAllRoles";
                            var rolesResult = httpClient.GetAsync(rolesUrl).Result;

                            if (rolesResult.IsSuccessStatusCode)
                            {
                                var rolesJson = rolesResult.Content.ReadAsStringAsync().Result;
                                roles = JsonSerializer.Deserialize<List<Rol>>(rolesJson, new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                }) ?? new List<Rol>();
                            }
                            ViewBag.Roles = roles;
                        }
                    }

                    return View(model);
                }

                [HttpPost]
                [AdminOnly]
                public IActionResult EliminarUsuario(int id)
                {
                    Console.WriteLine($"Intentando eliminar usuario con ID: {id}");

                    using (var httpClient = new HttpClient())
                    {
                        var url = $"http://localhost:5062/api/Account/DeleteUser/{id}";
                        Console.WriteLine($"URL de eliminación: {url}");

                        var result = httpClient.DeleteAsync(url).Result;

                        if (result.IsSuccessStatusCode)
                        {
                            var responseContent = result.Content.ReadAsStringAsync().Result;
                            Console.WriteLine("API Response: " + responseContent);

                            return Json(new
                            {
                                success = true,
                                message = "Usuario eliminado correctamente"
                            });
                        }
                        else
                        {
                            var errorContent = result.Content.ReadAsStringAsync().Result;
                            Console.WriteLine("API Error: " + errorContent);

                            return Json(new
                            {
                                success = false,
                                message = "Error al eliminar el usuario: " + errorContent
                            });
                        }
                    }
                }

                // roles
                [HttpGet]
                [AdminOnly]
                public IActionResult RegistrarRol()
                {
                    return View();
                }

                [HttpPost]
                [AdminOnly]
                public IActionResult RegistrarRol(Rol model)
                {
                    using (var httpClient = new HttpClient())
                    {
                        var url = "http://localhost:5062/api/Account/RegistrarRol";

                        var result = httpClient.PostAsJsonAsync(url, model).Result;

                        if (result.IsSuccessStatusCode)
                        {
                            TempData["Mensaje"] = "Rol creado correctamente";
                            return RedirectToAction("RegistrarRol");
                        }
                        else
                        {
                            var error = result.Content.ReadAsStringAsync().Result;
                            ViewBag.Error = "Error al registrar el rol: " + error;
                        }
                    }

                    return View(model);
                }


                [HttpGet]
                [AdminOnly]
                public IActionResult ListarRoles()
                {
                    List<Rol> lista = new List<Rol>();

                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            var url = "http://localhost:5062/api/Account/GetAllRoles";
                            Console.WriteLine($"Intentando conectar a: {url}");

                            var result = httpClient.GetAsync(url).Result;
                            Console.WriteLine($"Status Code: {result.StatusCode}");

                            if (result.IsSuccessStatusCode)
                            {
                                var json = result.Content.ReadAsStringAsync().Result;
                                Console.WriteLine($"Respuesta de la API: {json}");

                                lista = JsonSerializer.Deserialize<List<Rol>>(json, new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                }) ?? new List<Rol>();

                                Console.WriteLine($"Roles deserializados: {lista.Count}");
                            }
                            else
                            {
                                var errorContent = result.Content.ReadAsStringAsync().Result;
                                Console.WriteLine($"Error de la API: {errorContent}");
                                ViewBag.Error = $"Error al cargar roles. Status: {result.StatusCode}. Detalle: {errorContent}";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Excepción: {ex.Message}");
                        ViewBag.Error = $"Error de conexión: {ex.Message}";
                    }

                    return View(lista);
                }

                [HttpGet]
                [AdminOnly]
                public IActionResult EliminarRol(int id)
                {
                    using (var httpClient = new HttpClient())
                    {
                        var url = $"http://localhost:5062/api/Account/EliminarRol/{id}";
                        var result = httpClient.DeleteAsync(url).Result;

                        if (result.IsSuccessStatusCode)
                        {
                            TempData["Mensaje"] = "Rol eliminado correctamente.";
                        }
                        else
                        {
                            var errorContent = result.Content.ReadAsStringAsync().Result;
                            TempData["Error"] = "Error al eliminar el rol: " + errorContent;
                        }
                    }

                    return RedirectToAction("ListarRoles");
                }
                // roles

                [HttpPost]
                public IActionResult IniciarSesion(UsuarioModel model)
                {
                    if (!ModelState.IsValid)
                    {
                        return View(model);
                    }

                    using (var httpClient = new HttpClient())
                    {
                        var url = "http://localhost:5062/api/Account/Login";

                        var payload = new
                        {
                            correo = model.Correo,
                            contrasena = model.ContrasenaHash
                        };

                        var response = httpClient.PostAsJsonAsync(url, payload).Result;

                        var responseContent = response.Content.ReadAsStringAsync().Result;
                        _logger.LogInformation($"Respuesta del API de login: {responseContent}");

                        if (response.IsSuccessStatusCode)
                        {
                            try
                            {
                                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                                var apiResponse = JsonSerializer.Deserialize<LoginApiResponse>(responseContent, options);

                                if (apiResponse != null && apiResponse.Success)
                                {
                                    _logger.LogInformation("Login exitoso para {Correo}. Token recibido.", model.Correo);
                                    _logger.LogDebug("JWT: {Token}", apiResponse.Token);

                                    HttpContext.Session.SetString("JWToken", apiResponse.Token);
                                    if (apiResponse.User != null)
                                    {
                                        HttpContext.Session.SetInt32("IdUsuario", apiResponse.User.IdUsuario);
                                        HttpContext.Session.SetInt32("IdRol", apiResponse.User.IdRol);
                                        HttpContext.Session.SetString("NombreRol", apiResponse.User.NombreRol ?? string.Empty);
                                        HttpContext.Session.SetString("NombreCompleto", apiResponse.User.NombreCompleto ?? string.Empty);
                                    }

                                    return RedirectToAction("Index", "Home");
                                }
                                else
                                {
                                    _logger.LogWarning("Login fallido para {Correo}. success=false en respuesta.", model.Correo);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error al deserializar la respuesta del login");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Login fallido con status {StatusCode} para {Correo}", response.StatusCode, model.Correo);
                        }

                        ViewBag.Error = "Credenciales inválidas o error al iniciar sesión.";
                    }

                    return View(model);
                }
            }

        }
   