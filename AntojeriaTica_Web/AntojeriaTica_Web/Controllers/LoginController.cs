using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AntojeriaTica_Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        public LoginController(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
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
                    model.ContrasenaHash,
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
        public IActionResult Principal()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RecuperarContrasena()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecuperarContraseña(UsuarioModel model)
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

        [HttpGet]
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
        public IActionResult ActualizarUsuario(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("ListarUsuarios");
            }

            var model = new UsuarioModel();

            using (var httpClient = new HttpClient())
            {
                var url = $"http://localhost:5062/api/Account/GetUser/{id}";
                var result = httpClient.GetAsync(url).Result;

                if (result.IsSuccessStatusCode)
                {
                    var responseContent = result.Content.ReadAsStringAsync().Result;
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
                    var errorContent = result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("API Error: " + errorContent);
                    ViewBag.Error = "Error al obtener los datos del usuario: " + errorContent;
                    return RedirectToAction("ListarUsuarios");
                }
            }

            return View(model);
        }

        [HttpPost]
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
                }
            }

            return View(model);
        }

        [HttpPost]
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

    }
}
