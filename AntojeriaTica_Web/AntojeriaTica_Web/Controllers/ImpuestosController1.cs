using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AntojeriaTica_Web.Controllers
{
    public class ImpuestosController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ImpuestosController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> ConfiguracionImpuestos()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("http://localhost:5062/api/Impuestos/Listar");

            if (response.IsSuccessStatusCode)
            {
                var lista = await response.Content.ReadFromJsonAsync<List<Impuesto>>();
                return View(lista);
            }

            ViewBag.Error = "No se pudieron cargar los impuestos.";
            return View(new List<Impuesto>());
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PutAsync($"http://localhost:5062/api/Impuestos/CambiarEstado/{id}", null);
            return RedirectToAction("ConfiguracionImpuestos");
        }

        [HttpPost]
        public async Task<IActionResult> AgregarImpuesto(Impuesto nuevo)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("http://localhost:5062/api/Impuestos/Agregar", nuevo);
            return RedirectToAction("ConfiguracionImpuestos");
        }
    }
}
