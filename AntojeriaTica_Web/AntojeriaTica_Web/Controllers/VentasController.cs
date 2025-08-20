using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;

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
            if (model == null || model.Detalles == null || model.Detalles.Count == 0)
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto.");
                return View(model ?? new VentaModel { Detalles = new List<DetalleVentaModel>() });
            }

            var client = _httpClientFactory.CreateClient();
            var url = "http://localhost:5062/api/Ventas/RegistrarVenta";

            try
            {
                var response = await client.PostAsJsonAsync(url, model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Comprobante"] = "Venta registrada correctamente con método de pago: " + model.MetodoPago;
                    return RedirectToAction("Comprobante");
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error al registrar la venta: {content}");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error de conexión: {ex.Message}");
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


        [HttpGet]
        public async Task<IActionResult> BuscarVentas()
        {
            var model = new BusquedaVentasModel
            {
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddHours(23).AddMinutes(59)
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                var queryParams = new List<string>();

                if (model.FechaInicio.HasValue)
                    queryParams.Add($"fechaInicio={Uri.EscapeDataString(model.FechaInicio.Value.ToString("yyyy-MM-ddTHH:mm:ss"))}");

                if (model.FechaFin.HasValue)
                    queryParams.Add($"fechaFin={Uri.EscapeDataString(model.FechaFin.Value.ToString("yyyy-MM-ddTHH:mm:ss"))}");

                var queryString = string.Join("&", queryParams);
                var url = $"http://localhost:5062/api/Ventas/BuscarVentas?{queryString}";

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var ventas = await response.Content.ReadFromJsonAsync<List<VentaDetallada>>();
                    model.Resultados = ventas ?? new List<VentaDetallada>();
                }
                else
                {
                    model.Resultados = new List<VentaDetallada>();
                    ViewBag.Error = $"Error al cargar ventas iniciales. Status: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                model.Resultados = new List<VentaDetallada>();
                ViewBag.Error = $"Error de conexión: {ex.Message}";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BuscarVentas(BusquedaVentasModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var queryParams = new List<string>();

                if (model.FechaInicio.HasValue)
                    queryParams.Add($"fechaInicio={Uri.EscapeDataString(model.FechaInicio.Value.ToString("yyyy-MM-ddTHH:mm:ss"))}");
                
                if (model.FechaFin.HasValue)
                    queryParams.Add($"fechaFin={Uri.EscapeDataString(model.FechaFin.Value.ToString("yyyy-MM-ddTHH:mm:ss"))}");
                
                if (!string.IsNullOrEmpty(model.MetodoPago))
                    queryParams.Add($"metodoPago={Uri.EscapeDataString(model.MetodoPago)}");
                
                if (model.VentaId.HasValue)
                    queryParams.Add($"ventaId={model.VentaId.Value}");

                var queryString = string.Join("&", queryParams);
                var url = $"http://localhost:5062/api/Ventas/BuscarVentas?{queryString}";

                Console.WriteLine($"Llamando a URL: {url}");

                var response = await client.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response Status: {response.StatusCode}");
                Console.WriteLine($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var ventas = await response.Content.ReadFromJsonAsync<List<VentaDetallada>>();
                    model.Resultados = ventas ?? new List<VentaDetallada>();
                }
                else
                {
                    ViewBag.Error = $"Error al buscar las ventas. Status: {response.StatusCode}. Detalle: {responseContent}";
                    model.Resultados = new List<VentaDetallada>();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error de conexión: {ex.Message}";
                model.Resultados = new List<VentaDetallada>();
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DetalleVenta(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"http://localhost:5062/api/Ventas/DetalleVenta/{id}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var venta = await response.Content.ReadFromJsonAsync<VentaCompleta>();
                    return View(venta);
                }
                else
                {
                    ViewBag.Error = "No se pudo encontrar la venta solicitada.";
                    return View(new VentaCompleta());
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al cargar el detalle de la venta: {ex.Message}";
                return View(new VentaCompleta());
            }
        }

        [HttpGet]
        public async Task<IActionResult> ReporteVentasDia(DateTime? fecha = null)
        {
            var fechaConsulta = fecha ?? DateTime.Now.Date;

            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"http://localhost:5062/api/Ventas/ReporteVentasDia?fecha={fechaConsulta:yyyy-MM-dd}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var reporte = await response.Content.ReadFromJsonAsync<ReporteVentasDia>();
                    ViewBag.FechaConsulta = fechaConsulta;
                    return View(reporte);
                }
                else
                {
                    ViewBag.Error = "Error al generar el reporte de ventas.";
                    ViewBag.FechaConsulta = fechaConsulta;
                    return View(new ReporteVentasDia { Fecha = fechaConsulta });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al generar el reporte: {ex.Message}";
                ViewBag.FechaConsulta = fechaConsulta;
                return View(new ReporteVentasDia { Fecha = fechaConsulta });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ImprimirComprobante(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"http://localhost:5062/api/Ventas/DetalleVenta/{id}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var venta = await response.Content.ReadFromJsonAsync<VentaCompleta>();
                    return View(venta);
                }
                else
                {
                    TempData["Error"] = "No se pudo encontrar la venta para imprimir.";
                    return RedirectToAction("BuscarVentas");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el comprobante: {ex.Message}";
                return RedirectToAction("BuscarVentas");
            }
        }


        [HttpGet]
        public async Task<IActionResult> DevolucionTotal()
        {
            var model = new DevolucionTotalModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DevolucionTotal(DevolucionTotalModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = "http://localhost:5062/api/Devoluciones/ProcesarDevolucionTotal";

                var response = await client.PostAsJsonAsync(url, model);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<DevolucionResponseModel>();
                    if (resultado != null)
                    {
                        TempData["Success"] = $"Devolución procesada correctamente. Monto devuelto: ₡{resultado.MontoDevuelto:N0}";
                    }
                    else
                    {
                        TempData["Success"] = "Devolución procesada correctamente.";
                    }
                    return RedirectToAction("HistorialDevoluciones");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error al procesar devolución: {error}");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DevolucionParcial()
        {
            var model = new DevolucionParcialModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DevolucionParcial(DevolucionParcialModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = "http://localhost:5062/api/Devoluciones/ProcesarDevolucionParcial";

                var response = await client.PostAsJsonAsync(url, model);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<DevolucionResponseModel>();
                    if (resultado != null)
                    {
                        TempData["Success"] = $"Devolución parcial procesada. Monto devuelto: ₡{resultado.MontoDevuelto:N0}";
                    }
                    else
                    {
                        TempData["Success"] = "Devolución parcial procesada correctamente.";
                    }
                    return RedirectToAction("HistorialDevoluciones");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error al procesar devolución: {error}");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> HistorialDevoluciones()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = "http://localhost:5062/api/Devoluciones/HistorialDevoluciones";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var devoluciones = await response.Content.ReadFromJsonAsync<List<DevolucionDetalladaModel>>();
                    return View(devoluciones ?? new List<DevolucionDetalladaModel>());
                }
                else
                {
                    TempData["Error"] = "No se pudo cargar el historial de devoluciones.";
                    return View(new List<DevolucionDetalladaModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar historial: {ex.Message}";
                return View(new List<DevolucionDetalladaModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GestionarCreditos()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = "http://localhost:5062/api/Devoluciones/CreditosDisponibles";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var creditos = await response.Content.ReadFromJsonAsync<List<CreditoCliente>>();
                    return View(creditos ?? new List<CreditoCliente>());
                }
                else
                {
                    TempData["Error"] = "No se pudieron cargar los créditos disponibles.";
                    return View(new List<CreditoCliente>());
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar créditos: {ex.Message}";
                return View(new List<CreditoCliente>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AplicarCredito(AplicarCreditoModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = "http://localhost:5062/api/Devoluciones/AplicarCredito";

                var response = await client.PostAsJsonAsync(url, model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = $"Crédito aplicado correctamente. Monto: ₡{model.MontoAplicar:N0}";
                    return RedirectToAction("GestionarCreditos");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Error al aplicar crédito: {error}";
                    return RedirectToAction("GestionarCreditos");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("GestionarCreditos");
            }
        }

        [HttpGet]
    public async Task<JsonResult> ValidarVentaParaDevolucion(int ventaId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
        var url = $"http://localhost:5062/api/Devoluciones/ValidarVentaParaDevolucion/{ventaId}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var validacion = await response.Content.ReadFromJsonAsync<ValidacionVentaModel>();
                    return Json(validacion);
                }
                else
                {
                    return Json(new ValidacionVentaModel 
                    { 
                        EsValida = false, 
                        Mensaje = "No se pudo validar la venta" 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ValidacionVentaModel 
                { 
                    EsValida = false, 
                    Mensaje = $"Error: {ex.Message}" 
                });
            }
        }

        [HttpGet]
        public IActionResult EjemplosDevoluciones()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ProcesarDevoluciones()
        {
            var model = new DevolucionModel();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> VerCreditosCliente(string identificacion)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"http://localhost:5062/api/Devoluciones/CreditosDisponibles?identificacion={identificacion}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var creditos = await response.Content.ReadFromJsonAsync<List<CreditoCliente>>();
                    ViewBag.Identificacion = identificacion;
                    return View(creditos ?? new List<CreditoCliente>());
                }
                else
                {
                    TempData["Error"] = "No se pudieron cargar los créditos del cliente.";
                    return View(new List<CreditoCliente>());
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar créditos: {ex.Message}";
                return View(new List<CreditoCliente>());
            }
        }

        [HttpGet]
        public IActionResult BuscarVentaDevolucion()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> ListarVentasParaDevolucion(DateTime? fecha = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var dia = (fecha ?? DateTime.Today).Date;
                var inicio = new DateTime(dia.Year, dia.Month, dia.Day, 0, 0, 0);
                var fin = new DateTime(dia.Year, dia.Month, dia.Day, 23, 59, 59);

                var url = $"http://localhost:5062/api/Ventas/BuscarVentas?fechaInicio={Uri.EscapeDataString(inicio.ToString("yyyy-MM-ddTHH:mm:ss"))}&fechaFin={Uri.EscapeDataString(fin.ToString("yyyy-MM-ddTHH:mm:ss"))}";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return Json(new List<object>());
                }

                var ventas = await response.Content.ReadFromJsonAsync<List<VentaDetallada>>();
                var lista = (ventas ?? new List<VentaDetallada>()).Select(v => new
                {
                    id = v.Id,
                    label = $"#{v.Id} - {v.Fecha:dd/MM HH:mm} - ₡{v.Total.ToString("N0")} - {v.MetodoPago}"
                }).ToList();

                return Json(lista);
            }
            catch
            {
                return Json(new List<object>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarVentaDevolucionAjax(int ventaId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var urlDetalle = $"http://localhost:5062/api/Ventas/DetalleVenta/{ventaId}";
                var responseDetalle = await client.GetAsync(urlDetalle);
                if (responseDetalle.IsSuccessStatusCode)
                {
                    var venta = await responseDetalle.Content.ReadFromJsonAsync<VentaCompleta>();

                    var urlValidacion = $"http://localhost:5062/api/Devoluciones/ValidarVentaParaDevolucion/{ventaId}";
                    var responseValidacion = await client.GetAsync(urlValidacion);
                    if (responseValidacion.IsSuccessStatusCode)
                    {
                        var validacionJson = await responseValidacion.Content.ReadAsStringAsync();
                        var validacion = System.Text.Json.JsonSerializer.Deserialize<dynamic>(validacionJson);
                        var yaDevuelta = validacion.GetProperty("yaDevuelta").GetBoolean();
                        if (yaDevuelta)
                        {
                            return Json(new {
                                error = "Esta venta ya tiene devoluciones procesadas",
                                yaDevuelta = true,
                                venta
                            });
                        }
                    }

                    return Json(venta);
                }


                var urlValidacionFallback = $"http://localhost:5062/api/Devoluciones/ValidarVentaParaDevolucion/{ventaId}";
                var responseValidacionFallback = await client.GetAsync(urlValidacionFallback);
                if (responseValidacionFallback.IsSuccessStatusCode)
                {
                    var validacionJson = await responseValidacionFallback.Content.ReadAsStringAsync();
                    var validacion = System.Text.Json.JsonSerializer.Deserialize<dynamic>(validacionJson);
                    var esValida = validacion.GetProperty("valida").GetBoolean();
                    if (!esValida)
                    {
                        return Json(new { error = "Venta no válida para devolución" });
                    }
                }

                return Json(new { error = "Venta no encontrada" });
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Error al buscar la venta: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcesarDevolucion([FromBody] DevolucionRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                string url;
                
                if (request.TipoDevolucion == "Total")
                {
                    url = "http://localhost:5062/api/Devoluciones/ProcesarDevolucionTotal";
                    var devolucionTotal = new
                    {
                        VentaId = request.VentaId,
                        Motivo = request.Motivo,
                        TipoReembolso = "Efectivo"
                    };
                    
                    var response = await client.PostAsJsonAsync(url, devolucionTotal);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var resultado = await response.Content.ReadFromJsonAsync<DevolucionResponse>();
                        return Json(new { 
                            success = true, 
                            devolucionId = resultado?.Id,
                            montoDevuelto = request.MontoDevolucion 
                        });
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, error = $"Error en API Total: {errorContent}" });
                    }
                }
                else if (request.TipoDevolucion == "Parcial")
                {
                    url = "http://localhost:5062/api/Devoluciones/ProcesarDevolucionParcial";
                    
                    var devolucionParcial = new
                    {
                        VentaId = request.VentaId,
                        Motivo = request.Motivo,
                        TipoReembolso = "Efectivo",
                        ProductosDevolver = request.DetallesDevolucion.Select(d => new
                        {
                            ProductoId = d.ProductoId,
                            CantidadDevolver = d.CantidadDevuelta
                        }).ToList()
                    };
                    
                    var response = await client.PostAsJsonAsync(url, devolucionParcial);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var resultado = await response.Content.ReadFromJsonAsync<DevolucionResponse>();
                        return Json(new { 
                            success = true, 
                            devolucionId = resultado?.Id,
                            montoDevuelto = request.MontoDevolucion 
                        });
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, error = $"Error en API Parcial: {errorContent}" });
                    }
                }
                
                return Json(new { success = false, error = "Tipo de devolución no válido" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Error al procesar la devolución: {ex.Message}" });
            }
        }

    }

    public class DevolucionRequest
    {
        public int VentaId { get; set; }
        public string TipoDevolucion { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public decimal MontoDevolucion { get; set; }
        public List<DetalleDevolucionRequest> DetallesDevolucion { get; set; } = new List<DetalleDevolucionRequest>();
    }

    public class DetalleDevolucionRequest
    {
        public int ProductoId { get; set; }
        public int CantidadDevuelta { get; set; }
        public decimal MontoDevuelto { get; set; }
    }

    public class DevolucionResponse
    {
        public int Id { get; set; }
        public decimal MontoDevuelto { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
