using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace AntojeriaTica_Web.Controllers
{
    public class DevolucionesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DevolucionesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ValidarVenta(int ventaId, string tipoDevolucion)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"http://localhost:5062/api/Devoluciones/ValidarVentaParaDevolucion/{ventaId}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var ventaInfo = await response.Content.ReadFromJsonAsync<dynamic>();
                    
                    if (ventaInfo?.GetProperty("valida").GetBoolean() == true)
                    {
                        if (tipoDevolucion == "total")
                            return RedirectToAction("DevolucionTotal", new { ventaId });
                        else
                            return RedirectToAction("DevolucionParcial", new { ventaId });
                    }
                    else
                    {
                        TempData["Error"] = ventaInfo?.GetProperty("yaDevuelta").GetBoolean() == true 
                            ? "Esta venta ya ha sido devuelta anteriormente"
                            : "La venta no es válida para devolución";
                    }
                }
                else
                {
                    TempData["Error"] = "No se encontró la venta especificada o no es válida para devolución";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al validar la venta: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DevolucionTotal(int ventaId)
        {
            var model = new DevolucionTotalModel { VentaId = ventaId };
            
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"http://localhost:5062/api/Devoluciones/ValidarVentaParaDevolucion/{ventaId}";

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var ventaInfo = await response.Content.ReadFromJsonAsync<dynamic>();
                    if (ventaInfo != null)
                    {
                        model.FechaVenta = ventaInfo.GetProperty("fecha").GetDateTime();
                        model.MetodoPagoOriginal = ventaInfo.GetProperty("metodoPago").GetString();
                        model.MontoTotal = ventaInfo.GetProperty("total").GetDecimal();
                        model.CantidadProductos = ventaInfo.GetProperty("cantidadProductos").GetInt32();
                        model.VentaValida = ventaInfo.GetProperty("valida").GetBoolean();
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al cargar información de la venta: {ex.Message}";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DevolucionTotal(DevolucionTotalModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = "http://localhost:5062/api/Devoluciones/ProcesarDevolucionTotal";

                var request = new
                {
                    ventaId = model.VentaId,
                    tipoReembolso = model.TipoReembolso,
                    motivo = model.Motivo,
                    numeroIdentificacion = model.NumeroIdentificacion,
                    nombreCliente = model.NombreCliente,
                    diasVencimientoCredito = model.DiasVencimientoCredito
                };

                var response = await client.PostAsJsonAsync(url, request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<DevolucionResponseModel>();
                    TempData["Success"] = result?.Mensaje ?? "Devolución procesada exitosamente";
                    TempData["NumeroComprobante"] = result?.NumeroComprobante;
                    TempData["MontoDevuelto"] = result?.MontoDevuelto.ToString("C");
                    TempData["TipoReembolso"] = result?.TipoReembolso;
                    
                    if (result?.CreditoId.HasValue == true)
                    {
                        TempData["CreditoId"] = result.CreditoId.Value;
                    }

                    return RedirectToAction("ComprobanteDevolucion");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error al procesar la devolución: {error}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DevolucionParcial(int ventaId)
        {
            var model = new DevolucionParcialModel { VentaId = ventaId };
            
            try
            {
                var client = _httpClientFactory.CreateClient();
                
                var urlVenta = $"http://localhost:5062/api/Ventas/DetalleVenta/{ventaId}";
                var ventaResponse = await client.GetAsync(urlVenta);
                
                if (ventaResponse.IsSuccessStatusCode)
                {
                    var ventaCompleta = await ventaResponse.Content.ReadFromJsonAsync<VentaCompleta>();
                    if (ventaCompleta != null)
                    {
                        model.FechaVenta = ventaCompleta.Fecha;
                        model.MetodoPagoOriginal = ventaCompleta.MetodoPago;
                        model.MontoTotalVenta = ventaCompleta.Total;
                        
                        model.ProductosDisponibles = ventaCompleta.Detalles.Select(d => new ProductoDevolucionModel
                        {
                            ProductoId = d.ProductoId,
                            ProductoNombre = d.ProductoNombre,
                            ProductoCodigo = d.ProductoCodigo,
                            CantidadOriginal = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            CantidadDevolver = 0,
                            Seleccionado = false
                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al cargar información de la venta: {ex.Message}";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DevolucionParcial(DevolucionParcialModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = "http://localhost:5062/api/Devoluciones/ProcesarDevolucionParcial";

                var productosDevolver = model.ProductosDisponibles
                    .Where(p => p.Seleccionado && p.CantidadDevolver > 0)
                    .Select(p => new { productoId = p.ProductoId, cantidadDevolver = p.CantidadDevolver })
                    .ToList();

                if (!productosDevolver.Any())
                {
                    ModelState.AddModelError("", "Debe seleccionar al menos un producto para devolver");
                    return View(model);
                }

                var request = new
                {
                    ventaId = model.VentaId,
                    productosDevolver = productosDevolver,
                    tipoReembolso = model.TipoReembolso,
                    motivo = model.Motivo,
                    numeroIdentificacion = model.NumeroIdentificacion,
                    nombreCliente = model.NombreCliente,
                    diasVencimientoCredito = model.DiasVencimientoCredito
                };

                var response = await client.PostAsJsonAsync(url, request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<DevolucionResponseModel>();
                    TempData["Success"] = result?.Mensaje ?? "Devolución parcial procesada exitosamente";
                    TempData["NumeroComprobante"] = result?.NumeroComprobante;
                    TempData["MontoDevuelto"] = result?.MontoDevuelto.ToString("C");
                    TempData["TipoReembolso"] = result?.TipoReembolso;
                    
                    if (result?.CreditoId.HasValue == true)
                    {
                        TempData["CreditoId"] = result.CreditoId.Value;
                    }

                    return RedirectToAction("ComprobanteDevolucion");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error al procesar la devolución: {error}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult BuscarCreditos()
        {
            return View(new BuscarCreditosModel());
        }

        [HttpPost]
        public async Task<IActionResult> BuscarCreditos(BuscarCreditosModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"http://localhost:5062/api/Devoluciones/BuscarCreditosCliente/{model.NumeroIdentificacion}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var creditos = await response.Content.ReadFromJsonAsync<List<CreditoClienteModel>>();
                    model.CreditosDisponibles = creditos ?? new List<CreditoClienteModel>();
                }
                else
                {
                    ViewBag.Error = "No se encontraron créditos para el cliente especificado";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al buscar créditos: {ex.Message}";
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Historial()
        {
            return View(new BusquedaDevolucionesModel());
        }

        [HttpPost]
        public async Task<IActionResult> Historial(BusquedaDevolucionesModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var queryParams = new List<string>();

                if (model.FechaInicio.HasValue)
                    queryParams.Add($"fechaInicio={model.FechaInicio.Value:yyyy-MM-ddTHH:mm:ss}");
                
                if (model.FechaFin.HasValue)
                    queryParams.Add($"fechaFin={model.FechaFin.Value:yyyy-MM-ddTHH:mm:ss}");
                
                if (!string.IsNullOrEmpty(model.TipoDevolucion))
                    queryParams.Add($"tipoDevolucion={model.TipoDevolucion}");
                
                if (!string.IsNullOrEmpty(model.TipoReembolso))
                    queryParams.Add($"tipoReembolso={model.TipoReembolso}");

                var queryString = string.Join("&", queryParams);
                var url = $"http://localhost:5062/api/Devoluciones/HistorialDevoluciones?{queryString}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var devoluciones = await response.Content.ReadFromJsonAsync<List<DevolucionDetalladaModel>>();
                    model.Resultados = devoluciones ?? new List<DevolucionDetalladaModel>();
                }
                else
                {
                    ViewBag.Error = "Error al buscar el historial de devoluciones";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ComprobanteDevolucion()
        {
            return View();
        }
    }
}
