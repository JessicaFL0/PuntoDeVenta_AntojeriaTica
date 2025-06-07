using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AntojeriaTica_Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        public LoginController(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
        }

        #region Registrar Cuenta

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

        #endregion

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

        #region Actualizar Usuario

        [HttpGet]
        public IActionResult ActualizarUsuario(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("IniciarSesion");
            }

            using (var httpClient = new HttpClient())
            {
                var url = "http://localhost:5062/api/Account/GetUserById";
                var result = httpClient.GetAsync(url).Result;
                if (result.IsSuccessStatusCode)
                {
                    var user = result.Content.ReadFromJsonAsync<UsuarioModel>().Result;
                    return View(user);
                }
                else
                {
                    return RedirectToAction("IniciarSesion");
                }
            }
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

                var result = httpClient.PostAsJsonAsync(url, updateUserPayload).Result;

                if (result.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Usuario actualizado correctamente";
                    return RedirectToAction("Principal", "Login");
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

        #endregion
    }
}
