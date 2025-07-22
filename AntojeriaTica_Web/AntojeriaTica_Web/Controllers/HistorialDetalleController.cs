using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using AntojeriaTica_Web.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

public class HistorialDetalleController : Controller
{
    private readonly HttpClient _httpClient;

    public HistorialDetalleController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new System.Uri("http://localhost:5062/");
    }

    public async Task<IActionResult> Index()
    {
        var filtroVacio = new
        {
            FechaInicio = (DateTime?)null,
            FechaFin = (DateTime?)null,
            TipoOperacion = (string)null,
            Usuario = (string)null
        };

        var response = await _httpClient.PostAsJsonAsync("api/Historial/FiltrarHistorialDetalleVentas", filtroVacio);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var historialDetalle = JsonConvert.DeserializeObject<List<HistorialDetalleVenta>>(json);
            return View(historialDetalle);
        }
        else
        {
            ViewBag.Error = "No se pudo obtener el historial detalle";
            return View(new List<HistorialDetalleVenta>());
        }
    }
}
