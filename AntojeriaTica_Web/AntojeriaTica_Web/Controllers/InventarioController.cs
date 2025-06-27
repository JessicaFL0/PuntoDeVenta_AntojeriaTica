using AntojeriaTica_Web.Filters;
using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AntojeriaTica_Web.Controllers
{
    [AdminOnly]
    public class InventarioController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public InventarioController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult RegistrarProducto()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegistrarProducto(ProductoModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var client = new HttpClient();
            var url = "http://localhost:5062/api/Producto/RegistrarProducto";
            var response = client.PostAsJsonAsync(url, model).Result;
            var json = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Producto registrado correctamente";
                return RedirectToAction("ListarProductos");
            }

            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msgEl))
                {
                    ViewBag.Error = msgEl.GetString();
                }
                else if (doc.RootElement.TryGetProperty("error", out var errEl))
                {
                    ViewBag.Error = errEl.GetString();
                }
                else
                {
                    ViewBag.Error = $"Error ({response.StatusCode}) al registrar el producto: {json}";
                }
            }
            catch
            {
                ViewBag.Error = $"Error ({response.StatusCode}) al registrar el producto: {json}";
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ListarProductos()
        {
            List<ProductoModel> lista = new();
            using var client = new HttpClient();
            var url = "http://localhost:5062/api/Producto/ListarProductos";
            var response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                lista = JsonSerializer.Deserialize<List<ProductoModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            return View(lista);
        }

        [HttpGet]
        public IActionResult EditarProducto(int id)
        {
            using var client = new HttpClient();
            var url = "http://localhost:5062/api/Producto/ListarProductos"; // we fetch list then single
            var response = client.GetAsync(url).Result;
            ProductoModel? producto = null;
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var lista = JsonSerializer.Deserialize<List<ProductoModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                producto = lista.FirstOrDefault(p => p.IdProducto == id);
            }
            if (producto == null) return RedirectToAction("ListarProductos");
            return View(producto);
        }

        [HttpPost]
        public IActionResult EditarProducto(ProductoModel model)
        {
            if (!ModelState.IsValid) return View(model);
            using var client = new HttpClient();
            var url = "http://localhost:5062/api/Producto/ActualizarProducto";
            var response = client.PutAsJsonAsync(url, model).Result;
            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Producto actualizado";
                return RedirectToAction("ListarProductos");
            }
            var json = response.Content.ReadAsStringAsync().Result;
            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msgEl))
                {
                    ViewBag.Error = msgEl.GetString();
                }
                else if (doc.RootElement.TryGetProperty("error", out var errEl))
                {
                    ViewBag.Error = errEl.GetString();
                }
                else
                {
                    ViewBag.Error = $"Error ({response.StatusCode}) al actualizar el producto: {json}";
                }
            }
            catch
            {
                ViewBag.Error = $"Error ({response.StatusCode}) al actualizar el producto: {json}";
            }
            return View(model);
        }
    }
}
