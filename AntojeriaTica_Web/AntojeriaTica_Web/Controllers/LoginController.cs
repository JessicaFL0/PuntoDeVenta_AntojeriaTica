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
            using (var api = _httpClient.CreateClient())
            {
                var url = "https://localhost:7243/api/Login/RegistrarCuenta";
                var result = api.PostAsJsonAsync(url, model).Result;

                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("IniciarSesion", "Login");
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
        public IActionResult RecuperarContrasena ()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecuperarContraseña(UsuarioModel model)
        {
            using (var api = _httpClient.CreateClient())
            {
                var url = "https://localhost:7232/api/Login/RecuperarContrasenna";
                var result = api.PostAsJsonAsync(url, model).Result;

                if (result.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Se ha enviado un enlace de recuperación a tu correo.";
                    return RedirectToAction("IniciarSesion");
                }
            }

            ViewBag.Error = "No se pudo enviar el enlace. Verifica el correo.";
            return View(model);
        }
    }
}
