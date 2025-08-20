using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using AntojeriaTica_Web.Models;

namespace AntojeriaTica_Web.Controllers
{
    public class FacturacionElectronicaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public FacturacionElectronicaController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: FacturacionElectronica/GenerarDesdeVenta
        public IActionResult GenerarDesdeVenta(int ventaId)
        {
            var model = new GenerarFacturaElectronicaViewModel
            {
                VentaId = ventaId
            };
            return View(model);
        }

        // POST: FacturacionElectronica/GenerarDesdeVenta
        [HttpPost]
        public async Task<IActionResult> GenerarDesdeVenta(GenerarFacturaElectronicaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5062";

                var requestData = new
                {
                    VentaId = model.VentaId,
                    ClienteNombre = model.ClienteNombre,
                    ClienteEmail = model.ClienteEmail,
                    ClienteTelefono = model.ClienteTelefono,
                    ClienteIdentificacion = model.ClienteIdentificacion
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/FacturaElectronica/GenerarFactura", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var facturaResponse = JsonSerializer.Deserialize<FacturaElectronicaResponseModel>(responseContent, options);

                    TempData["SuccessMessage"] = $"Factura electrónica generada exitosamente. Número: {facturaResponse.NumeroFactura}";
                    return RedirectToAction("Index", "FacturacionElectronica");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error al generar la factura: {errorContent}");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                return View(model);
            }
        }

        // GET: FacturacionElectronica
        public async Task<IActionResult> Index()
        {
            var model = new BusquedaFacturasElectronicasViewModel
            {
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today
            };

            // Cargar resultados iniciales (hoy)
            await BuscarFacturas(model);
            return View(model);
        }

        // POST: FacturacionElectronica/BuscarFacturas
        [HttpPost]
        public async Task<IActionResult> BuscarFacturas(BusquedaFacturasElectronicasViewModel model)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5062";

                var queryParams = new List<string>();
                
                if (model.FechaInicio.HasValue)
                    queryParams.Add($"fechaInicio={model.FechaInicio.Value:yyyy-MM-dd}");
                
                if (model.FechaFin.HasValue)
                    queryParams.Add($"fechaFin={model.FechaFin.Value:yyyy-MM-dd}");
                
                if (!string.IsNullOrEmpty(model.NumeroFactura))
                    queryParams.Add($"numeroFactura={Uri.EscapeDataString(model.NumeroFactura)}");
                
                if (!string.IsNullOrEmpty(model.ClienteNombre))
                    queryParams.Add($"clienteNombre={Uri.EscapeDataString(model.ClienteNombre)}");
                
                if (!string.IsNullOrEmpty(model.EstadoHacienda))
                    queryParams.Add($"estadoHacienda={Uri.EscapeDataString(model.EstadoHacienda)}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/FacturaElectronica/BuscarFacturas{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var facturas = JsonSerializer.Deserialize<List<FacturaElectronicaModel>>(responseContent, options);
                    model.Facturas = facturas ?? new List<FacturaElectronicaModel>();
                }
                else
                {
                    model.Facturas = new List<FacturaElectronicaModel>();
                    TempData["ErrorMessage"] = "Error al buscar las facturas electrónicas";
                }
            }
            catch (Exception ex)
            {
                model.Facturas = new List<FacturaElectronicaModel>();
                TempData["ErrorMessage"] = $"Error inesperado: {ex.Message}";
            }

            return View("Index", model);
        }

        // GET: FacturacionElectronica/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5062";

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/FacturaElectronica/DetalleFactura/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var detalle = JsonSerializer.Deserialize<DetalleFacturaElectronicaModel>(responseContent, options);
                    return View(detalle);
                }
                else
                {
                    TempData["ErrorMessage"] = "Factura electrónica no encontrada";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al obtener el detalle: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: FacturacionElectronica/ReenviarEmail
        [HttpPost]
        public async Task<JsonResult> ReenviarEmail(int idFactura, string email)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5062";

                var requestData = new { Email = email };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/FacturaElectronica/ReenviarFactura/{idFactura}", content);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Email reenviado exitosamente" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Error al reenviar email: {errorContent}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error inesperado: {ex.Message}" });
            }
        }

        // GET: FacturacionElectronica/DescargarPDF/5
        public async Task<IActionResult> DescargarPDF(int id)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5062";

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/FacturaElectronica/DescargarPDF/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                    return File(pdfBytes, "application/pdf", $"Factura-{id}.pdf");
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al generar el PDF de la factura";
                    return RedirectToAction("Detalle", new { id });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al descargar PDF: {ex.Message}";
                return RedirectToAction("Detalle", new { id });
            }
        }
    }
}
