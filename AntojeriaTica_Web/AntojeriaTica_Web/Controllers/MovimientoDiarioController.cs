using AntojeriaTica_Web.Filters;
using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;

namespace AntojeriaTica_Web.Controllers
{
    [AdminOnly]
    public class MovimientoDiarioController : Controller
    {
        [HttpGet]
        public IActionResult Registrar()
        {
            ViewBag.Categorias = new List<string> { "Ventas", "Compras", "Gastos Operativos" };
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(MovimientoDiario model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = new List<string> { "Ventas", "Compras", "Gastos Operativos" };
                return View(model);
            }

            if (HttpContext.Session.GetInt32("IdUsuario") is int idUsuario)
            {
                model.IdUsuario = idUsuario;
            }
            else
            {
                ViewBag.Error = "No se encontr� el usuario en sesi�n.";
                return View(model);
            }

            using var client = new HttpClient();
            var response = client.PostAsJsonAsync("http://localhost:5062/api/MovimientoDiario/registrar", model).Result;
            var json = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Movimiento registrado correctamente";
                return RedirectToAction("Listar");
            }

            try
            {
                var doc = JsonDocument.Parse(json);
                ViewBag.Error = doc.RootElement.GetProperty("mensaje").GetString();
            }
            catch
            {
                ViewBag.Error = "Error al registrar el movimiento.";
            }

            ViewBag.Categorias = new List<string> { "Ventas", "Compras", "Gastos Operativos" };
            return View(model);
        }

        [HttpGet]
        public IActionResult Listar()
        {
            List<MovimientoDiario> lista = new();
            using var client = new HttpClient();
            var response = client.GetAsync("http://localhost:5062/api/MovimientoDiario/listar").Result;

            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                lista = JsonSerializer.Deserialize<List<MovimientoDiario>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            else
            {
                ViewBag.Error = "No se pudo obtener la lista de movimientos.";
            }

            return View(lista);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            using var client = new HttpClient();
            var response = client.GetAsync("http://localhost:5062/api/MovimientoDiario/listar").Result;

            if (!response.IsSuccessStatusCode) return RedirectToAction("Listar");

            var json = response.Content.ReadAsStringAsync().Result;
            var lista = JsonSerializer.Deserialize<List<MovimientoDiario>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            var movimiento = lista.FirstOrDefault(m => m.IdMovimiento == id);

            if (movimiento == null) return RedirectToAction("Listar");

            ViewBag.Categorias = new List<string> { "Ventas", "Compras", "Gastos Operativos" };
            return View(movimiento);
        }

        [HttpPost]
        public IActionResult Editar(MovimientoDiario model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = new List<string> { "Ventas", "Compras", "Gastos Operativos" };
                return View(model);
            }

            using var client = new HttpClient();
            var response = client.PostAsJsonAsync("http://localhost:5062/api/MovimientoDiario/actualizar", model).Result;
            var json = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Movimiento actualizado correctamente";
                return RedirectToAction("Listar");
            }

            try
            {
                var doc = JsonDocument.Parse(json);
                ViewBag.Error = doc.RootElement.GetProperty("mensaje").GetString();
            }
            catch
            {
                ViewBag.Error = "Error al actualizar el movimiento.";
            }

            ViewBag.Categorias = new List<string> { "Ventas", "Compras", "Gastos Operativos" };
            return View(model);
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            using var client = new HttpClient();
            var content = JsonContent.Create(id);
            var response = client.PostAsync("http://localhost:5062/api/MovimientoDiario/eliminar", content).Result;
            var json = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Movimiento eliminado correctamente";
            }
            else
            {
                try
                {
                    var doc = JsonDocument.Parse(json);
                    TempData["Error"] = doc.RootElement.GetProperty("mensaje").GetString();
                }
                catch
                {
                    TempData["Error"] = "Error al eliminar el movimiento.";
                }
            }

            return RedirectToAction("Listar");
        }
    }
}
