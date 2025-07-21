using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AntojeriaTica_Web.Controllers
{
    public class VentasController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public VentasController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        
        [HttpGet]
        public IActionResult RegistrarVenta()
        {
            return View(new VentaModel { Detalles = new List<DetalleVentaModel>() });
        }

        
        [HttpPost]
        public async Task<IActionResult> RegistrarVenta(VentaModel model)
        {
            var client = _httpClientFactory.CreateClient();
            var url = "http://localhost:5062/api/Ventas/RegistrarVenta"; 

            var response = await client.PostAsJsonAsync(url, model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Comprobante"] = "Venta registrada correctamente con método de pago: " + model.MetodoPago;
                return RedirectToAction("Comprobante");
            }
            else
            {
                ModelState.AddModelError("", "Error al registrar la venta.");
                return View(model);
            }
        }

        
        public IActionResult Comprobante()
        {
            ViewBag.Mensaje = TempData["Comprobante"]?.ToString();
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ConfiguracionPago()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("http://localhost:5062/api/MetodoPago/Listar");

            if (response.IsSuccessStatusCode)
            {
                var metodos = await response.Content.ReadFromJsonAsync<List<MetodoPago>>();
                return View(metodos);
            }

            ViewBag.Error = "No se pudieron cargar los métodos de pago.";
            return View(new List<MetodoPago>());
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int idMetodo)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PutAsync($"http://localhost:5062/api/MetodoPago/CambiarEstado/{idMetodo}", null);

            return RedirectToAction("ConfiguracionPago");
        }

        [HttpPost]
        public async Task<IActionResult> AgregarMetodoPago(string nombre)
        {
            var client = _httpClientFactory.CreateClient();
            var nuevo = new MetodoPago { Nombre = nombre };

            var response = await client.PostAsJsonAsync("http://localhost:5062/api/MetodoPago/Agregar", nuevo);

            return RedirectToAction("ConfiguracionPago");
        }

        [HttpPost]
        public async Task<IActionResult> VerHistorial(int idMetodo)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://localhost:5062/api/MetodoPago/Historial/{idMetodo}");

            if (response.IsSuccessStatusCode)
            {
                var historial = await response.Content.ReadFromJsonAsync<List<HistorialMetodoPago>>();
                ViewBag.IdMetodo = idMetodo;
                return View("HistorialMetodoPago", historial);
            }

            ViewBag.Error = "No se pudo cargar el historial.";
            return RedirectToAction("ConfiguracionPago");
        }

        
        [HttpGet]
        public async Task<IActionResult> GestionarDescuentos()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("http://localhost:5062/api/Descuento/Listar");

            if (response.IsSuccessStatusCode)
            {
                var lista = await response.Content.ReadFromJsonAsync<List<DescuentoModel>>();
                return View(lista);
            }

            ViewBag.Error = "No se pudieron cargar los descuentos.";
            return View(new List<DescuentoModel>());
        }

        
        [HttpPost]
        public async Task<IActionResult> AgregarDescuento(DescuentoModel model)
        {
            var client = _httpClientFactory.CreateClient();
            model.Estado = "Activo"; 
            var response = await client.PostAsJsonAsync("http://localhost:5062/api/Descuento/Agregar", model);

            return RedirectToAction("GestionarDescuentos");
        }

        
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoDescuento(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PutAsync($"http://localhost:5062/api/Descuento/CambiarEstado/{id}", null);

            return RedirectToAction("GestionarDescuentos");
        }

    }
}
