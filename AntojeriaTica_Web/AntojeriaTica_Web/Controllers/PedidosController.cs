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
            var url = "http://localhost:5062/api/Pedidos/BuscarPedidos";

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
                    ModelState.AddModelError("", $"Error al registrar el pedido: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error de conexión: {ex.Message}");
            }

            await CargarListasAsync();
            return View(model);
        }

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
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var pedido = JsonSerializer.Deserialize<PedidoModel>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (pedido != null)
                    {
                        return View(pedido);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el detalle del pedido: {ex.Message}";
            }

            return RedirectToAction("Index");
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

        // POST: Pedidos/ActualizarEstado
        [HttpPost]
        public async Task<IActionResult> ActualizarEstado(int pedidoId, string nuevoEstado)
        {
            var client = _httpClientFactory.CreateClient();
            var url = "http://localhost:5062/api/Pedidos/ActualizarEstado";

            var request = new
            {
                PedidoId = pedidoId,
                NuevoEstado = nuevoEstado,
                UsuarioId = 1 // Esto debería venir de la sesión del usuario
            };

            try
            {
                var response = await client.PostAsJsonAsync(url, request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Estado del pedido actualizado correctamente";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Error al actualizar el estado: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error de conexión: {ex.Message}";
            }

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
                var productosUrl = "http://localhost:5062/api/Producto/ObtenerProductos";
                var productosResponse = await client.GetAsync(productosUrl);
                
                if (productosResponse.IsSuccessStatusCode)
                {
                    var productosJson = await productosResponse.Content.ReadAsStringAsync();
                    var productos = JsonSerializer.Deserialize<List<ProductoModel>>(productosJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    ViewBag.Productos = new SelectList(productos ?? new List<ProductoModel>(), "Id", "Nombre");
                }
            }
            catch
            {
                ViewBag.Productos = new SelectList(new List<ProductoModel>(), "Id", "Nombre");
            }

            // Cargar tipos de pedido
            ViewBag.TiposPedido = new SelectList(new List<string> { "Mesa", "Telefono", "App" });
        }
    }
}