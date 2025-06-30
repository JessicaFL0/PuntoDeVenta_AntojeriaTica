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
    }
}
