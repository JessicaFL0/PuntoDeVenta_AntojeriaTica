using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AntojeriaTica_Web.Controllers
{
    public class CierreCajaController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            CierreCaja model = new();
            using var client = new HttpClient();
            var response = client.GetAsync("http://localhost:5062/api/CierreCaja/obtenerTotalesHoy").Result;

            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var data = JsonSerializer.Deserialize<CierreCaja>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (data != null) model = data;
            }
            else
            {
                ViewBag.Error = "No se pudieron cargar los datos del cierre de caja.";
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Index(CierreCaja model)
        {
            if (model.HayDiferencias && string.IsNullOrWhiteSpace(model.NotaJustificativa))
            {
                ViewBag.Error = "Debe justificar la diferencia detectada.";
                return View(model);
            }

            TempData["Mensaje"] = "Cierre de caja realizado correctamente.";
            return RedirectToAction("Index");
        }
   
    [HttpGet]
    public IActionResult Listar()
    {
        List<CierreCaja> lista = new();
        using var client = new HttpClient();
        var response = client.GetAsync("http://localhost:5062/api/CierreCaja/listar").Result;

        if (response.IsSuccessStatusCode)
        {
            var json = response.Content.ReadAsStringAsync().Result;
            lista = JsonSerializer.Deserialize<List<CierreCaja>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
        else
        {
            ViewBag.Error = "No se pudo obtener la lista de cierres.";
        }

            return View("Listar", lista);
        }

}
}