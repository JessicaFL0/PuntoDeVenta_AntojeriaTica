using AntojeriaTica_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;

namespace AntojeriaTica_Web.Controllers
{
    public class ReporteController : Controller
    {
        private readonly string apiUrl = "http://localhost:5062/api/Reporte/";


        [HttpGet]
        public IActionResult VentasAnuales(int anio = 2024)
        {
            List<ReporteVentasAnualesResponse> model = ObtenerDatos(anio);

            ViewBag.Anio = anio;
            return View("VentasAnuales", model);
        }

        public IActionResult ExportarPDF(int anio = 2024)
        {
            List<ReporteVentasAnualesResponse> model = ObtenerDatos(anio);

            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                doc.Add(new Paragraph($"Reporte de Ventas del Año {anio}\n\n"));

                PdfPTable table = new PdfPTable(3);
                table.AddCell("Mes");
                table.AddCell("Total Ventas");
                table.AddCell("Cantidad Ventas");

                foreach (var item in model)
                {
                    table.AddCell(item.NombreMes);
                    table.AddCell(item.TotalVentas.ToString("N2"));
                    table.AddCell(item.CantidadVentas.ToString());
                }

                doc.Add(table);
                doc.Close();

                return File(ms.ToArray(), "application/pdf", $"ReporteVentas_{anio}.pdf");
            }
        }

        public IActionResult ExportarHTML(int anio = 2024)
        {
            List<ReporteVentasAnualesResponse> model = ObtenerDatos(anio);

            StringBuilder sb = new StringBuilder();
            sb.Append($"<h2>Reporte de Ventas del Año {anio}</h2>");
            sb.Append("<table border='1' style='border-collapse:collapse;width:100%'>");
            sb.Append("<tr><th>Mes</th><th>Total Ventas</th><th>Cantidad Ventas</th></tr>");

            foreach (var item in model)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{item.NombreMes}</td>");
                sb.Append($"<td>{item.TotalVentas:N2}</td>");
                sb.Append($"<td>{item.CantidadVentas}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</table>");

            byte[] fileBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(fileBytes, "text/html", $"ReporteVentas_{anio}.html");
        }

        private List<ReporteVentasAnualesResponse> ObtenerDatos(int anio)
        {
            List<ReporteVentasAnualesResponse> model = new();
            using (var client = new HttpClient())
            {
                var response = client.GetAsync($"{apiUrl}ventas-anuales/{anio}").Result;
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    model = JsonSerializer.Deserialize<List<ReporteVentasAnualesResponse>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ReporteVentasAnualesResponse>();
                }
            }
            return model;
        }


        [HttpGet]
        public IActionResult Dashboard()
        {
            DashboardModel dashboard = new();

            using (var client = new HttpClient())
            {
                var response = client.GetAsync($"{apiUrl}dashboard").Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    dashboard = JsonSerializer.Deserialize<DashboardModel>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new DashboardModel();
                }
                else
                {
                    ViewBag.Error = "No se pudieron cargar los datos del dashboard.";
                }
            }

            return View("Dashboard", dashboard);
        }


        [HttpGet]
        public IActionResult GenerarReporte(string busqueda = "")
        {
            List<VentaModel> ventas = new();

            using (var client = new HttpClient())
            {
                var response = client.GetAsync("http://localhost:5062/api/Venta/listar").Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    ventas = JsonSerializer.Deserialize<List<VentaModel>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<VentaModel>();
                }
            }


            if (!string.IsNullOrEmpty(busqueda))
            {
                ventas = ventas.Where(v =>
                    (v.MetodoPago ?? "").Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    (v.CodigoCupon ?? "").Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    v.Detalles.Any(d => d.ProductoId.ToString().Contains(busqueda))
                ).ToList();
            }

            return View(ventas);
        }


        [HttpGet]
        public IActionResult ExportarReporte()
        {
            List<VentaModel> ventas = ObtenerVentas();

            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                doc.Add(new Paragraph("Reporte de Ventas Detallado\n\n"));

                PdfPTable table = new PdfPTable(4);
                table.AddCell("Producto ID");
                table.AddCell("Cantidad");
                table.AddCell("Precio Unitario");
                table.AddCell("Método Pago");

                foreach (var venta in ventas)
                {
                    foreach (var detalle in venta.Detalles)
                    {
                        table.AddCell(detalle.ProductoId.ToString());
                        table.AddCell(detalle.Cantidad.ToString());
                        table.AddCell(detalle.PrecioUnitario.ToString("N2"));
                        table.AddCell(venta.MetodoPago ?? "");
                    }
                }

                doc.Add(table);
                doc.Close();

                return File(ms.ToArray(), "application/pdf", "ReporteVentasDetallado.pdf");
            }
        }

        [HttpGet]
        public IActionResult ExportarReporteHTML()
        {
            List<VentaModel> ventas = ObtenerVentas();

            StringBuilder sb = new StringBuilder();
            sb.Append("<h2>Reporte de Ventas Detallado</h2>");
            sb.Append("<table border='1' style='border-collapse:collapse;width:100%'>");
            sb.Append("<tr><th>Producto ID</th><th>Cantidad</th><th>Precio Unitario</th><th>Método Pago</th><th>Cupón</th></tr>");

            foreach (var venta in ventas)
            {
                foreach (var detalle in venta.Detalles)
                {
                    sb.Append("<tr>");
                    sb.Append($"<td>{detalle.ProductoId}</td>");
                    sb.Append($"<td>{detalle.Cantidad}</td>");
                    sb.Append($"<td>{detalle.PrecioUnitario:N2}</td>");
                    sb.Append($"<td>{venta.MetodoPago}</td>");
                    sb.Append($"<td>{venta.CodigoCupon}</td>");
                    sb.Append("</tr>");
                }
            }

            sb.Append("</table>");

            byte[] fileBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(fileBytes, "text/html", "ReporteVentasDetallado.html");
        }

        private List<VentaModel> ObtenerVentas()
        {
            List<VentaModel> ventas = new();
            using (var client = new HttpClient())
            {
                var response = client.GetAsync("http://localhost:5062/api/Venta/listar").Result;
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    ventas = JsonSerializer.Deserialize<List<VentaModel>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<VentaModel>();
                }
            }
            return ventas;
        }
    }
}
