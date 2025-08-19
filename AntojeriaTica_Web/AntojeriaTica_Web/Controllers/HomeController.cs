using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace AntojeriaTica_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var rol = HttpContext.Session.GetString("NombreRol") ?? string.Empty;
            var nombre = HttpContext.Session.GetString("NombreCompleto");
            var isLogged = !string.IsNullOrEmpty(rol);
            var isAdmin = rol.ToLowerInvariant().Contains("admin");

            var model = new HomeIndexViewModel
            {
                IsLogged = isLogged,
                IsAdmin = isAdmin,
                Rol = rol,
                NombreUsuario = nombre
            };

            if (isAdmin)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var response = await client.GetAsync("http://localhost:5062/api/Reporte/dashboard");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var dashboard = JsonSerializer.Deserialize<DashboardModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        model.Dashboard = dashboard;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cargando dashboard para Home/Index");
                }
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
