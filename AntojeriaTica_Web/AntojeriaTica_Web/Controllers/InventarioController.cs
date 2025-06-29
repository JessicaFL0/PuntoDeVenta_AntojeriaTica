using AntojeriaTica_Web.Filters;
using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
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


       
        [HttpPost]
        public IActionResult EliminarProducto(int id)
        {
            using var client = new HttpClient();
            var url = $"http://localhost:5062/api/Producto/{id}";
            var response = client.DeleteAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Producto eliminado correctamente";
            }
            else
            {
                var json = response.Content.ReadAsStringAsync().Result;
                try
                {
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("message", out var msgEl))
                    {
                        TempData["Error"] = msgEl.GetString();
                    }
                    else if (doc.RootElement.TryGetProperty("error", out var errEl))
                    {
                        TempData["Error"] = errEl.GetString();
                    }
                    else
                    {
                        TempData["Error"] = $"Error ({response.StatusCode}) al eliminar el producto: {json}";
                    }
                }
                catch
                {
                    TempData["Error"] = $"Error ({response.StatusCode}) al eliminar el producto: {json}";
                }
            }

            return RedirectToAction("ListarProductos");
        }

        [HttpGet]
        private List<ProductoModel> ObtenerProductos()
        {
            List<ProductoModel> productos = new();
            using var client = new HttpClient();
            var response = client.GetAsync("http://localhost:5062/api/Producto/ListarProductos").Result;
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                productos = JsonSerializer.Deserialize<List<ProductoModel>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();
            }
            return productos;
        }

        [HttpGet]
        public IActionResult RegistrarMovimiento()
        {
            List<ProductoModel> productos = new();
            using var client = new HttpClient();
            var response = client.GetAsync("http://localhost:5062/api/Producto/ListarProductos").Result;
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                productos = JsonSerializer.Deserialize<List<ProductoModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            ViewBag.Productos = productos;
            return View();
        }

        [HttpPost]
        public IActionResult RegistrarMovimiento(MovimientoInventario model)
        {
            if (model.CantidadEsperada.HasValue && model.Cantidad != model.CantidadEsperada.Value)
            {
                ViewBag.Error = "La cantidad recibida no coincide con la esperada.";
                ViewData["Titulo"] = "Registrar Movimiento";
                ViewData["Productos"] = ObtenerProductos(); 
                return View(model);
            }

            using var client = new HttpClient();
            var response = client.PostAsJsonAsync("http://localhost:5062/api/Producto/RegistrarMovimiento", model).Result;
            var json = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Movimiento registrado correctamente";
                return RedirectToAction("EstadoInventario");
            }

            try
            {
                var doc = JsonDocument.Parse(json);
                ViewBag.Error = doc.RootElement.GetProperty("message").GetString();
            }
            catch
            {
                ViewBag.Error = "Error al registrar el movimiento.";
            }

            ViewData["Titulo"] = "Registrar Movimiento";
            ViewData["Productos"] = ObtenerProductos();
            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> EstadoInventario(string filtro = "todos")
        {
            using var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:5062/api/Producto/ListarProductosConEstado");


            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "No se pudo obtener la información del inventario.";
                return View(new List<ProductoConEstadoViewModel>());
            }

            var products = await response.Content.ReadFromJsonAsync<List<ProductoConEstadoViewModel>>();
            if (products == null) products = new List<ProductoConEstadoViewModel>();

            if (filtro == "bajo")
            {
                products = products
                    .Where(p => p.EstadoStock == "Agotado" || p.EstadoStock == "Bajo stock")
                    .ToList();
            }

            return View(products);
        }



    }
}
