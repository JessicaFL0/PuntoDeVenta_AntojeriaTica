using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;
using System.Text.Json;

namespace AntojeriaTica_Web.Controllers
{
    public class PedidosController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PedidosController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: Pedidos
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var rol = HttpContext.Session.GetString("NombreRol") ?? string.Empty;
            var isAdmin = rol.ToLowerInvariant().Contains("admin");
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            string url = "http://localhost:5062/api/Pedidos/BuscarPedidos";
            if (!isAdmin && idUsuario.HasValue)
            {
                url += $"?usuarioId={idUsuario.Value}";
            }

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

        // GET: Pedidos/RegistrarPedido
        [HttpGet]
        public async Task<IActionResult> RegistrarPedido()
        {
            await CargarListasAsync();
            return View(new PedidoModel { Detalles = new List<DetallePedidoModel>() });
        }

        // POST: Pedidos/RegistrarPedido
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

            // Simular UsuarioId del usuario logueado (esto debería venir de la sesión)
            model.UsuarioId = 1;

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
                    
                    // Intentar deserializar el error para obtener un mensaje más específico
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
                        // Si no se puede deserializar, mostrar el error raw
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

        // GET: Pedidos/ActualizarEstado/5
        public async Task<IActionResult> ActualizarEstado(int id)
        {
            var client = _httpClientFactory.CreateClient();
            
            // Obtener estados disponibles
            var estadosUrl = "http://localhost:5062/api/Pedidos/ObtenerEstados";
            var estadosResponse = await client.GetAsync(estadosUrl);
            
            if (estadosResponse.IsSuccessStatusCode)
            {
                var estadosJson = await estadosResponse.Content.ReadAsStringAsync();
                var estados = JsonSerializer.Deserialize<List<string>>(estadosJson);
                ViewBag.Estados = new SelectList(estados);
            }

            // Obtener información del pedido
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

        // GET: Pedidos/Cocina - Vista especial para la cocina
        public async Task<IActionResult> Cocina()
        {
            var client = _httpClientFactory.CreateClient();
            var url = "http://localhost:5062/api/Pedidos/BuscarPedidos?estado=En preparación";

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

        // Método para cargar listas desplegables
        private async Task CargarListasAsync()
        {
            var client = _httpClientFactory.CreateClient();

            // Cargar productos
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

                    // Pasar la lista completa de productos para acceder al precio
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

            // Cargar tipos de pedido
            ViewBag.TiposPedido = new SelectList(new List<string> { "Mesa", "Telefono", "App" });
        }

        // GET: Pedidos/Seguimiento - PED-002
        public IActionResult Seguimiento()
        {
            // Permitir acceso sin autenticación (solo lectura)
            // Los usuarios no autenticados pueden ver pedidos pero no notificaciones personalizadas
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");
            Console.WriteLine($"Seguimiento - Usuario ID: {idUsuario}");
            
            // Agregar información del usuario a ViewBag para usar en la vista si es necesario
            ViewBag.UsuarioAutenticado = idUsuario.HasValue && idUsuario.Value > 0;
            ViewBag.IdUsuario = idUsuario ?? 0;

            return View();
        }

        // GET: Pedidos/Detalle/{id} - TEMPORALMENTE DESHABILITADO
        // GET: Pedidos/Detalle/5
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

        // POST: Pedidos/ActualizarEstado (maneja tanto formularios HTML como AJAX)
        [HttpPost]
        public async Task<IActionResult> ActualizarEstado(int pedidoId, string nuevoEstado)
        {
            // Debug: mostrar los datos recibidos
            Console.WriteLine($"ActualizarEstado - pedidoId: {pedidoId}, nuevoEstado: '{nuevoEstado}'");
            
            // Validar que se haya proporcionado un nuevo estado
            if (string.IsNullOrWhiteSpace(nuevoEstado))
            {
                var errorMessage = "Debe seleccionar un nuevo estado para el pedido.";
                Console.WriteLine($"Error: {errorMessage}");
                
                // Si es una petición AJAX, devolver JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                else
                {
                    // Si es un formulario HTML, redirigir con error
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
                    // Si es una petición AJAX, devolver JSON
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Estado actualizado correctamente" });
                    }
                    else
                    {
                        // Si es un formulario HTML, redirigir con mensaje
                        TempData["Success"] = "Estado del pedido actualizado correctamente";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error del API - Status: {response.StatusCode}");
                    Console.WriteLine($"Error del API - Content: {errorContent}");
                    
                    // Intentar deserializar el error para obtener un mensaje más específico
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
                            // Manejar errores de validación
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

                    // Si es una petición AJAX, devolver JSON
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = errorMessage });
                    }
                    else
                    {
                        // Si es un formulario HTML, redirigir con error
                        TempData["Error"] = errorMessage;
                        return RedirectToAction("ActualizarEstado", new { id = pedidoId });
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error de conexión: {ex.Message}";
                
                // Si es una petición AJAX, devolver JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }
                else
                {
                    // Si es un formulario HTML, redirigir con error
                    TempData["Error"] = errorMessage;
                    return RedirectToAction("ActualizarEstado", new { id = pedidoId });
                }
            }
        }

        // Endpoints para seguimiento - PED-002
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

        // Método proxy para obtener detalles de un pedido para AJAX
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