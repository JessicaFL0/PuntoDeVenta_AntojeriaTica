using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace AntojeriaTica_Web.Controllers
{
    public class CierreMensualController : Controller
    {
        private readonly HttpClient _client;

        public CierreMensualController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5062/api/CierreMensual/");
        }

        // Listar todos los cierres mensuales
        public async Task<IActionResult> Listar()
        {
            var response = await _client.GetAsync("listar");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "No se pudo obtener la lista de cierres mensuales.";
                return View(new List<CierreFinancieroMensual>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var lista = JsonConvert.DeserializeObject<List<CierreFinancieroMensual>>(json) ?? new List<CierreFinancieroMensual>();
            return View(lista);
        }

        // Mostrar formulario para generar cierre mensual
        // GET: CierreMensual/Registrar
        [HttpGet]
        public IActionResult Registrar()
        {
            var modelo = new CierreFinancieroMensual();

            
            modelo.GeneradoPor = User.Identity?.Name ?? "Desconocido";

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int mes, int anio)
        {
            var response = await _client.DeleteAsync($"eliminar?mes={mes}&anio={anio}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Listar");
            }

            ViewBag.Error = "Error al intentar eliminar el cierre.";
            return RedirectToAction("Listar");
        }

        // Registrar cierre mensual
        [HttpPost]
        public async Task<IActionResult> Registrar(CierreFinancieroMensual cierre)
        {
            
            cierre.GeneradoPor = User.Identity?.Name ?? "Desconocido";

            var json = JsonConvert.SerializeObject(new
            {
                Mes = cierre.Mes,
                Anio = cierre.Anio,
                Usuario = cierre.GeneradoPor,
                Comentario = cierre.ComentarioJustificativo
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("registrar", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Cierre mensual generado correctamente.";
                return RedirectToAction("Listar");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var errorObj = JsonConvert.DeserializeObject<dynamic>(responseContent);
            ViewBag.Error = errorObj?.mensaje ?? "Error al registrar el cierre.";

            return View(cierre);
        }

        public async Task<IActionResult> Comparacion()
        {
            var response = await _client.GetAsync("listar");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "No se pudo obtener la información para comparar.";
                return View(new List<CierreFinancieroMensual>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var lista = JsonConvert.DeserializeObject<List<CierreFinancieroMensual>>(json)
                ?.OrderBy(c => c.Anio).ThenBy(c => c.Mes).ToList();

            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarComentario(int mes, int anio, string comentario)
        {
            var json = JsonConvert.SerializeObject(new
            {
                Mes = mes,
                Anio = anio,
                Comentario = comentario
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PutAsync("actualizarComentario", content);

            return RedirectToAction("Comparacion");
        }

    }

}
