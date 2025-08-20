using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Net.Http;
using AntojeriaTica_Web.Filters;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;
using System.Text.Json;
using System.Linq;

namespace AntojeriaTica_Web.Controllers
{
    public class PedidosController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PedidosController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var rolRedir = HttpContext.Session.GetString("NombreRol") ?? string.Empty;
            if (rolRedir.Equals("Cocina", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Cocina");
            }

            var client = _httpClientFactory.CreateClient();
            var rol = HttpContext.Session.GetString("NombreRol") ?? string.Empty;
            var roleNorm = rol.ToLowerInvariant();
                var isPrivileged = roleNorm.Contains("admin") || roleNorm.Contains("cajero") || roleNorm.Contains("vendedor");
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            var fechaInicio = DateTime.Today;
            var fechaFin = DateTime.Today.AddDays(1).AddSeconds(-1);

            var query = new List<string>
            {
                $"fechaInicio={Uri.EscapeDataString(fechaInicio.ToString("yyyy-MM-ddTHH:mm:ss"))}",
                $"fechaFin={Uri.EscapeDataString(fechaFin.ToString("yyyy-MM-ddTHH:mm:ss"))}"
            };
            if (!isPrivileged && idUsuario.HasValue)
            {
                query.Add($"usuarioId={idUsuario.Value}");
            }
            string url = $"http://localhost:5062/api/Pedidos/BuscarPedidos?{string.Join("&", query)}";

            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var pedidos = JsonSerializer.Deserialize<List<PedidoResumenModel>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return View(pedidos ?? new List<PedidoResumenModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar pedidos: {ex.Message}";
            }

            return View(new List<PedidoResumenModel>());
        }

        [HttpGet]
        public async Task<IActionResult> RegistrarPedido()
        {
            await CargarListasAsync();
            return View(new PedidoModel { Detalles = new List<DetallePedidoModel>() });
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarPedido(PedidoModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarListasAsync();
                return View(model);
            }

            var client = _httpClientFactory.CreateClient();
            var url = "http://localhost:5062/api/Pedidos/RegistrarPedido";

            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");
            if (!idUsuario.HasValue || idUsuario.Value <= 0)
            {
                ModelState.AddModelError("", "No se pudo determinar el usuario en sesión. Inicie sesión nuevamente.");
                await CargarListasAsync();
                return View(model);
            }
            model.UsuarioId = idUsuario.Value;

            try
            {
                var response = await client.PostAsJsonAsync(url, model);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var resultado = JsonSerializer.Deserialize<dynamic>(jsonResponse);
                    
                    TempData["Success"] = "Pedido registrado correctamente";
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                        
                        if (errorObj.TryGetProperty("error", out var errorMessage))
                        {
                            ModelState.AddModelError("", $"Error del servidor: {errorMessage.GetString()}");
                        }
                        else if (errorObj.TryGetProperty("message", out var message))
                        {
                            ModelState.AddModelError("", $"Error: {message.GetString()}");
                        }
                        else
                        {
                            ModelState.AddModelError("", $"Error al registrar el pedido. Código de estado: {response.StatusCode}");
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("", $"Error al registrar el pedido: {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error de conexión: {ex.Message}");
            }

            await CargarListasAsync();
            return View(model);
        }

    [AdminOnly("Admin","Cocina")]
        public async Task<IActionResult> ActualizarEstado(int id)
        {
            var client = _httpClientFactory.CreateClient();
            
            var estadosUrl = "http://localhost:5062/api/Pedidos/ObtenerEstados";
            var estadosResponse = await client.GetAsync(estadosUrl);
            
            if (estadosResponse.IsSuccessStatusCode)
            {
                var estadosJson = await estadosResponse.Content.ReadAsStringAsync();
                var estados = JsonSerializer.Deserialize<List<string>>(estadosJson);
                ViewBag.Estados = new SelectList(estados);
            }

            var pedidoUrl = $"http://localhost:5062/api/Pedidos/ObtenerDetalle/{id}";
            var pedidoResponse = await client.GetAsync(pedidoUrl);
            if (pedidoResponse.IsSuccessStatusCode)
            {
                var pedidoJson = await pedidoResponse.Content.ReadAsStringAsync();
                var pedido = JsonSerializer.Deserialize<PedidoModel>(pedidoJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (pedido != null)
                {
                    ViewBag.PedidoInfo = pedido;
                    ViewBag.PedidoId = id;
                    ViewBag.EstadoActual = pedido.Estado;
                    return View();
                }
            }

            TempData["Error"] = "No se pudo cargar la información del pedido";
            return RedirectToAction("Index");
        }

    [AdminOnly("Admin","Cocina","Cajero","Vendedor")]
        public async Task<IActionResult> Cocina()
        {
            var client = _httpClientFactory.CreateClient();
            var estado = Uri.EscapeDataString("En preparación");
            var url = $"http://localhost:5062/api/Pedidos/BuscarPedidos?estado={estado}";

            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var pedidos = JsonSerializer.Deserialize<List<PedidoResumenModel>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return View(pedidos ?? new List<PedidoResumenModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar pedidos de cocina: {ex.Message}";
            }

            return View(new List<PedidoResumenModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var client = _httpClientFactory.CreateClient();

            var infoUrl = $"http://localhost:5062/api/Pedidos/InfoBasica/{id}";
            try
            {
                var infoResp = await client.GetAsync(infoUrl);
                if (!infoResp.IsSuccessStatusCode)
                {
                    TempData["Error"] = "No se pudo cargar el pedido o no existe";
                    return RedirectToAction("Index");
                }

                var infoJson = await infoResp.Content.ReadAsStringAsync();
                var info = JsonSerializer.Deserialize<PedidoBasicoInfo>(infoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (info == null)
                {
                    TempData["Error"] = "Pedido no encontrado";
                    return RedirectToAction("Index");
                }

                var rol = HttpContext.Session.GetString("NombreRol") ?? string.Empty;
                var esPrivilegiado = rol.Equals("Admin", StringComparison.OrdinalIgnoreCase) || rol.Equals("Cocina", StringComparison.OrdinalIgnoreCase);
                var idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

                if (!esPrivilegiado)
                {
                    var minutos = (int)(DateTime.Now - info.Fecha).TotalMinutes;
                    if (info.UsuarioId != idUsuario || minutos > 5)
                    {
                        TempData["Error"] = "No tiene permisos para editar este pedido o se superó la ventana de 5 minutos";
                        return RedirectToAction("Index");
                    }
                }

                var detalleUrl = $"http://localhost:5062/api/Pedidos/ObtenerDetalle/{id}";
                var detResp = await client.GetAsync(detalleUrl);
                if (!detResp.IsSuccessStatusCode)
                {
                    TempData["Error"] = "No se pudo cargar el detalle del pedido";
                    return RedirectToAction("Index");
                }

                var detJson = await detResp.Content.ReadAsStringAsync();
                var pedido = JsonSerializer.Deserialize<PedidoModel>(detJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PedidoModel();
                await CargarListasAsync();
                ViewBag.MinutosTranscurridos = (int)(DateTime.Now - info.Fecha).TotalMinutes;
                return View(pedido);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error cargando pedido: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(PedidoModel model)
        {
            if (model == null || model.Id <= 0)
            {
                TempData["Error"] = "Datos inválidos";
                return RedirectToAction("Index");
            }

            var client = _httpClientFactory.CreateClient();
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            var urlBasico = $"http://localhost:5062/api/Pedidos/EditarBasico/{model.Id}";
            var bodyBasico = new { UsuarioId = idUsuario, Cliente = model.Cliente, Mesa = model.Mesa, Observaciones = model.Observaciones };
            var urlProductos = $"http://localhost:5062/api/Pedidos/EditarProductos/{model.Id}";
            var bodyProductos = new
            {
                UsuarioId = idUsuario,
                Detalles = ((model.Detalles ?? new List<DetallePedidoModel>())
                            .Select(d => (object)new {
                                ProductoId = d.ProductoId,
                                Cantidad = d.Cantidad,
                                PrecioUnitario = d.PrecioUnitario,
                                ObservacionesItem = d.ObservacionesItem
                            }).ToList())
            };

            try
            {
                var respBasico = await client.PutAsJsonAsync(urlBasico, bodyBasico);
                string contentBasico = await respBasico.Content.ReadAsStringAsync();

                if (!respBasico.IsSuccessStatusCode)
                {
                    try
                    {
                        var errObj = JsonSerializer.Deserialize<JsonElement>(contentBasico);
                        string msg = errObj.TryGetProperty("message", out var m) ? m.GetString() : contentBasico;
                        TempData["Error"] = msg;
                    }
                    catch { TempData["Error"] = contentBasico; }
                    return RedirectToAction("Editar", new { id = model.Id });
                }
                if (model.Detalles != null && model.Detalles.Count > 0)
                {
                    var respProd = await client.PutAsJsonAsync(urlProductos, bodyProductos);
                    var contentProd = await respProd.Content.ReadAsStringAsync();
                    if (!respProd.IsSuccessStatusCode)
                    {
                        try
                        {
                            var errObj = JsonSerializer.Deserialize<JsonElement>(contentProd);
                            string msg = errObj.TryGetProperty("message", out var m) ? m.GetString() : contentProd;
                            TempData["Error"] = "Básico actualizado, pero productos: " + msg;
                        }
                        catch { TempData["Error"] = "Básico actualizado, pero productos: " + contentProd; }
                        return RedirectToAction("Editar", new { id = model.Id });
                    }
                }

                TempData["Success"] = "Pedido actualizado";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error de conexión: {ex.Message}";
                return RedirectToAction("Editar", new { id = model.Id });
            }
        }

        private async Task CargarListasAsync()
        {
            var client = _httpClientFactory.CreateClient();

            try
            {
                var productosUrl = "http://localhost:5062/api/Producto/ListarProductos";
                var productosResponse = await client.GetAsync(productosUrl);
                
                if (productosResponse.IsSuccessStatusCode)
                {
                    var productosJson = await productosResponse.Content.ReadAsStringAsync();
                    var productos = JsonSerializer.Deserialize<List<ProductoModel>>(productosJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    ViewBag.ProductosCompletos = productos ?? new List<ProductoModel>();
                    ViewBag.Productos = new SelectList(productos ?? new List<ProductoModel>(), "IdProducto", "Nombre");
                }
                else
                {
                    Console.WriteLine($"Error al cargar productos: {productosResponse.StatusCode}");
                    ViewBag.ProductosCompletos = new List<ProductoModel>();
                    ViewBag.Productos = new SelectList(new List<ProductoModel>(), "IdProducto", "Nombre");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al cargar productos: {ex.Message}");
                ViewBag.ProductosCompletos = new List<ProductoModel>();
                ViewBag.Productos = new SelectList(new List<ProductoModel>(), "IdProducto", "Nombre");
            }

            ViewBag.TiposPedido = new SelectList(new List<string> { "Mesa", "Telefono", "App" });
        }

        public IActionResult Seguimiento()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");
            Console.WriteLine($"Seguimiento - Usuario ID: {idUsuario}");
            
            ViewBag.UsuarioAutenticado = idUsuario.HasValue && idUsuario.Value > 0;
            ViewBag.IdUsuario = idUsuario ?? 0;

            return View();
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"http://localhost:5062/api/Pedidos/ObtenerDetalle/{id}";

            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var pedido = JsonSerializer.Deserialize<PedidoModel>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return View(pedido);
                }
                else
                {
                    TempData["Error"] = "No se pudo cargar el detalle del pedido";
                    return RedirectToAction("Seguimiento");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el detalle: {ex.Message}";
                return RedirectToAction("Seguimiento");
            }
        }

    [HttpPost]
    [AdminOnly("Admin","Cocina")]
        public async Task<IActionResult> ActualizarEstado(int pedidoId, string nuevoEstado)
        {
            Console.WriteLine($"ActualizarEstado - pedidoId: {pedidoId}, nuevoEstado: '{nuevoEstado}'");
            
            var estado = Uri.EscapeDataString(nuevoEstado);
            if (string.IsNullOrWhiteSpace(nuevoEstado))
            {
                var errorMessage = "Debe seleccionar un nuevo estado para el pedido.";
                Console.WriteLine($"Error: {errorMessage}");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                else
                {
                    TempData["Error"] = errorMessage;
                    return RedirectToAction("ActualizarEstado", new { id = pedidoId });
                }
            }

            var client = _httpClientFactory.CreateClient();
            var url = $"http://localhost:5062/api/Pedidos/ActualizarEstadoSimple/{pedidoId}";

            try
            {
                var usuarioId = HttpContext.Session.GetInt32("IdUsuario") ?? 1;
                Console.WriteLine($"Usuario ID obtenido de sesión: {usuarioId}");
                
                var requestData = new
                {
                    NuevoEstado = nuevoEstado.Trim(),
                    UsuarioId = usuarioId
                };

                Console.WriteLine($"Datos a enviar: NuevoEstado='{requestData.NuevoEstado}', UsuarioId={requestData.UsuarioId}");
                Console.WriteLine($"URL del API: {url}");

                var response = await client.PostAsJsonAsync(url, requestData);
                
                if (response.IsSuccessStatusCode)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Estado actualizado correctamente" });
                    }
                    else
                    {
                        TempData["Success"] = "Estado del pedido actualizado correctamente";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error del API - Status: {response.StatusCode}");
                    Console.WriteLine($"Error del API - Content: {errorContent}");
                    
                    string errorMessage;
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                        
                        if (errorObj.TryGetProperty("message", out var message))
                        {
                            errorMessage = message.GetString();
                        }
                        else if (errorObj.TryGetProperty("errors", out var errors))
                        {
                            var errorMessages = new List<string>();
                            foreach (var error in errors.EnumerateObject())
                            {
                                if (error.Value.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var errorMsg in error.Value.EnumerateArray())
                                    {
                                        errorMessages.Add(errorMsg.GetString());
                                    }
                                }
                            }
                            errorMessage = string.Join(", ", errorMessages);
                        }
                        else
                        {
                            errorMessage = $"Error del servidor (código {response.StatusCode})";
                        }
                    }
                    catch
                    {
                        errorMessage = $"Error del servidor (código {response.StatusCode})";
                    }

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = errorMessage });
                    }
                    else
                    {
                        TempData["Error"] = errorMessage;
                        return RedirectToAction("ActualizarEstado", new { id = pedidoId });
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error de conexión: {ex.Message}";
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                else
                {
                    TempData["Error"] = errorMessage;
                    return RedirectToAction("ActualizarEstado", new { id = pedidoId });
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPedidosConSeguimiento(int? usuarioId = null, bool soloAtrasados = false)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"http://localhost:5062/api/Pedidos/ObtenerPedidosConSeguimiento?";
            
            var parameters = new List<string>();
            if (usuarioId.HasValue)
                parameters.Add($"usuarioId={usuarioId.Value}");
            if (soloAtrasados)
                parameters.Add($"soloAtrasados={soloAtrasados}");
            
            url += string.Join("&", parameters);

            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var pedidos = JsonSerializer.Deserialize<List<object>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return Json(pedidos ?? new List<object>());
                }
                else
                {
                    return Json(new List<object>());
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerNotificaciones(int usuarioId, bool soloNoLeidas = false)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"http://localhost:5062/api/Pedidos/ObtenerNotificaciones/{usuarioId}?soloNoLeidas={soloNoLeidas}";

            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var notificaciones = JsonSerializer.Deserialize<List<object>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return Json(notificaciones ?? new List<object>());
                }
                else
                {
                    return Json(new List<object>());
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DetectarPedidosAtrasados()
        {
            var client = _httpClientFactory.CreateClient();
            var url = "http://localhost:5062/api/Pedidos/DetectarPedidosAtrasados";

            try
            {
                var response = await client.PostAsync(url, null);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var resultado = JsonSerializer.Deserialize<object>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return Json(resultado);
                }
                else
                {
                    return Json(new { error = "Error al detectar pedidos atrasados" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarcarNotificacionLeida(int notificacionId, [FromBody] int usuarioId)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"http://localhost:5062/api/Pedidos/MarcarNotificacionLeida/{notificacionId}";

            try
            {
                var jsonContent = JsonContent.Create(usuarioId);
                var response = await client.PostAsync(url, jsonContent);
                
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, error = "Error al marcar notificación como leída" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/Pedidos/DetalleJson/{id:int}")]
        public async Task<IActionResult> ObtenerDetallePedidoJson(int id)
        {
            Console.WriteLine($"Web - ObtenerDetallePedidoJson llamado con id: {id}");
            Console.WriteLine($"Web - Request URL: {Request.Path}");
            Console.WriteLine($"Web - Request Method: {Request.Method}");
            
            var client = _httpClientFactory.CreateClient();
            var url = $"http://localhost:5062/api/Pedidos/ObtenerDetalle/{id}";

            try
            {
                Console.WriteLine($"Web - Llamando API: {url}");
                var response = await client.GetAsync(url);
                
                Console.WriteLine($"Web - Response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Web - Response content length: {jsonResponse.Length}");
                    
                    var detalle = JsonSerializer.Deserialize<object>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return Json(detalle);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Web - Error response: {errorContent}");
                    return Json(new { success = false, error = "Error al obtener detalles del pedido" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}