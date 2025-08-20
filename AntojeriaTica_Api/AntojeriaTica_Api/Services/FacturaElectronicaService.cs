using iTextSharp.text;
using iTextSharp.text.pdf;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Data.SqlClient;
using MimeKit;
using System.Data;
using System.Text;

namespace AntojeriaTica_Api.Services
{
    public interface IFacturaElectronicaService
    {
        byte[] GenerarPDFFactura(int idFactura);
        Task<bool> EnviarEmailFacturaAsync(int idFactura, string emailDestino);
    }

    public class FacturaElectronicaService : IFacturaElectronicaService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public FacturaElectronicaService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public byte[] GenerarPDFFactura(int idFactura)
        {
            using var memoryStream = new MemoryStream();
            
            var document = new Document(PageSize.A4, 50, 50, 50, 50);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();

            var facturaData = ObtenerDatosFactura(idFactura);
            if (facturaData == null)
            {
                throw new Exception($"Factura con ID {idFactura} no encontrada");
            }

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(0, 0, 255));
            var title = new Paragraph("FACTURA ELECTRÓNICA", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(title);

            var empresaFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            
            document.Add(new Paragraph("ANTOJERÍA TICA", empresaFont));
            document.Add(new Paragraph("Cédula Jurídica: 3-101-123456", normalFont));
            document.Add(new Paragraph("Teléfono: 2222-3333", normalFont));
            document.Add(new Paragraph("Email: info@antojeriatica.com", normalFont));
            document.Add(new Paragraph(" ", normalFont));

            var facturaInfoTable = new PdfPTable(2) { WidthPercentage = 100 };
            facturaInfoTable.SetWidths(new float[] { 1, 1 });

            facturaInfoTable.AddCell(new PdfPCell(new Phrase("Número de Factura:", empresaFont)) { Border = 0 });
            facturaInfoTable.AddCell(new PdfPCell(new Phrase(facturaData.NumeroFactura, normalFont)) { Border = 0 });
            
            facturaInfoTable.AddCell(new PdfPCell(new Phrase("Clave Numérica:", empresaFont)) { Border = 0 });
            facturaInfoTable.AddCell(new PdfPCell(new Phrase(facturaData.ClaveNumerica, normalFont)) { Border = 0 });
            
            facturaInfoTable.AddCell(new PdfPCell(new Phrase("Fecha de Emisión:", empresaFont)) { Border = 0 });
            facturaInfoTable.AddCell(new PdfPCell(new Phrase(facturaData.FechaEmision.ToString("dd/MM/yyyy HH:mm"), normalFont)) { Border = 0 });
            
            facturaInfoTable.AddCell(new PdfPCell(new Phrase("Estado Hacienda:", empresaFont)) { Border = 0 });
            facturaInfoTable.AddCell(new PdfPCell(new Phrase(facturaData.EstadoHacienda, normalFont)) { Border = 0 });

            document.Add(facturaInfoTable);
            document.Add(new Paragraph(" ", normalFont));

            var clienteTitle = new Paragraph("DATOS DEL CLIENTE", empresaFont) { SpacingBefore = 10, SpacingAfter = 10 };
            document.Add(clienteTitle);

            var clienteTable = new PdfPTable(2) { WidthPercentage = 100 };
            clienteTable.SetWidths(new float[] { 1, 2 });

            clienteTable.AddCell(new PdfPCell(new Phrase("Nombre:", empresaFont)) { Border = 0 });
            clienteTable.AddCell(new PdfPCell(new Phrase(facturaData.ClienteNombre, normalFont)) { Border = 0 });
            
            if (!string.IsNullOrEmpty(facturaData.ClienteEmail))
            {
                clienteTable.AddCell(new PdfPCell(new Phrase("Email:", empresaFont)) { Border = 0 });
                clienteTable.AddCell(new PdfPCell(new Phrase(facturaData.ClienteEmail, normalFont)) { Border = 0 });
            }
            
            if (!string.IsNullOrEmpty(facturaData.ClienteTelefono))
            {
                clienteTable.AddCell(new PdfPCell(new Phrase("Teléfono:", empresaFont)) { Border = 0 });
                clienteTable.AddCell(new PdfPCell(new Phrase(facturaData.ClienteTelefono, normalFont)) { Border = 0 });
            }

            document.Add(clienteTable);
            document.Add(new Paragraph(" ", normalFont));

            var detalleTitle = new Paragraph("DETALLE DE PRODUCTOS", empresaFont) { SpacingBefore = 10, SpacingAfter = 10 };
            document.Add(detalleTitle);

            var detalleTable = new PdfPTable(4) { WidthPercentage = 100 };
            detalleTable.SetWidths(new float[] { 3, 1, 2, 2 });

            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, new BaseColor(255, 255, 255));
            var headerCell1 = new PdfPCell(new Phrase("Producto", headerFont)) { BackgroundColor = new BaseColor(0, 0, 255), Padding = 5 };
            var headerCell2 = new PdfPCell(new Phrase("Cant.", headerFont)) { BackgroundColor = new BaseColor(0, 0, 255), Padding = 5 };
            var headerCell3 = new PdfPCell(new Phrase("Precio Unit.", headerFont)) { BackgroundColor = new BaseColor(0, 0, 255), Padding = 5 };
            var headerCell4 = new PdfPCell(new Phrase("Total", headerFont)) { BackgroundColor = new BaseColor(0, 0, 255), Padding = 5 };

            detalleTable.AddCell(headerCell1);
            detalleTable.AddCell(headerCell2);
            detalleTable.AddCell(headerCell3);
            detalleTable.AddCell(headerCell4);

            var productos = ObtenerProductosFactura(idFactura);
            foreach (var producto in productos)
            {
                detalleTable.AddCell(new PdfPCell(new Phrase(producto.NombreProducto, normalFont)) { Padding = 5 });
                detalleTable.AddCell(new PdfPCell(new Phrase(producto.Cantidad.ToString(), normalFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                detalleTable.AddCell(new PdfPCell(new Phrase(producto.PrecioUnitario.ToString("C"), normalFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                detalleTable.AddCell(new PdfPCell(new Phrase((producto.Cantidad * producto.PrecioUnitario).ToString("C"), normalFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            document.Add(detalleTable);
            document.Add(new Paragraph(" ", normalFont));

            var totalesTable = new PdfPTable(2) { WidthPercentage = 60, HorizontalAlignment = Element.ALIGN_RIGHT };
            totalesTable.SetWidths(new float[] { 1, 1 });

            totalesTable.AddCell(new PdfPCell(new Phrase("Subtotal:", empresaFont)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
            totalesTable.AddCell(new PdfPCell(new Phrase(facturaData.SubTotal.ToString("C"), normalFont)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
            
            totalesTable.AddCell(new PdfPCell(new Phrase("Impuestos:", empresaFont)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
            totalesTable.AddCell(new PdfPCell(new Phrase(facturaData.MontoImpuesto.ToString("C"), normalFont)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
            
            totalesTable.AddCell(new PdfPCell(new Phrase("TOTAL:", empresaFont)) { Border = Rectangle.TOP_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
            totalesTable.AddCell(new PdfPCell(new Phrase(facturaData.MontoTotal.ToString("C"), empresaFont)) { Border = Rectangle.TOP_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

            document.Add(totalesTable);

            document.Add(new Paragraph(" ", normalFont));
            document.Add(new Paragraph("Gracias por su compra", normalFont) { Alignment = Element.ALIGN_CENTER });

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<bool> EnviarEmailFacturaAsync(int idFactura, string emailDestino)
        {
            try
            {
                var facturaData = ObtenerDatosFactura(idFactura);
                if (facturaData == null) return false;

                var pdfContent = GenerarPDFFactura(idFactura);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Antojería Tica", "noreply@antojeriatica.com"));
                message.To.Add(new MailboxAddress("", emailDestino));
                message.Subject = $"Factura Electrónica {facturaData.NumeroFactura}";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2 style='color: #2E86C1;'>Factura Electrónica</h2>
                        <p>Estimado/a {facturaData.ClienteNombre},</p>
                        <p>Adjuntamos su factura electrónica con los siguientes datos:</p>
                        <ul>
                            <li><strong>Número de Factura:</strong> {facturaData.NumeroFactura}</li>
                            <li><strong>Fecha de Emisión:</strong> {facturaData.FechaEmision:dd/MM/yyyy HH:mm}</li>
                            <li><strong>Total:</strong> {facturaData.MontoTotal:C}</li>
                        </ul>
                        <p>Si tiene alguna consulta, no dude en contactarnos.</p>
                        <br>
                        <p><strong>Antojería Tica</strong><br>
                        Teléfono: 2222-3333<br>
                        Email: info@antojeriatica.com</p>
                    </body>
                    </html>"
                };

                bodyBuilder.Attachments.Add($"Factura-{facturaData.NumeroFactura}.pdf", pdfContent, ContentType.Parse("application/pdf"));
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                await Task.Delay(1000);
                
                
                RegistrarEnvioEmail(idFactura, emailDestino, "Enviado", "Email enviado exitosamente");
                
                return true;
            }
            catch (Exception ex)
            {
                RegistrarEnvioEmail(idFactura, emailDestino, "Error", ex.Message);
                return false;
            }
        }

        private dynamic? ObtenerDatosFactura(int idFactura)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

         var sql = @"
          SELECT f.IdFactura, f.NumeroFactura, f.ClaveNumerica, f.ClienteNombre, 
              f.ClienteEmail, f.ClienteTelefono, f.FechaEmision, f.SubTotal, 
              f.MontoImpuesto, f.MontoTotal, f.EstadoHacienda
          FROM FacturaElectronica f 
          WHERE f.Id = @IdFactura";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@IdFactura", idFactura);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new
                {
                    IdFactura = reader.GetInt32("IdFactura"),
                    NumeroFactura = reader.GetString("NumeroFactura"),
                    ClaveNumerica = reader.GetString("ClaveNumerica"),
                    ClienteNombre = reader.GetString("ClienteNombre"),
                    ClienteEmail = reader.IsDBNull("ClienteEmail") ? "" : reader.GetString("ClienteEmail"),
                    ClienteTelefono = reader.IsDBNull("ClienteTelefono") ? "" : reader.GetString("ClienteTelefono"),
                    FechaEmision = reader.GetDateTime("FechaEmision"),
                    SubTotal = reader.GetDecimal("SubTotal"),
                    MontoImpuesto = reader.GetDecimal("MontoImpuesto"),
                    MontoTotal = reader.GetDecimal("MontoTotal"),
                    EstadoHacienda = reader.GetString("EstadoHacienda")
                };
            }

            return null;
        }

        private List<dynamic> ObtenerProductosFactura(int idFactura)
        {
            var productos = new List<dynamic>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var sql = @"
                SELECT p.Nombre as NombreProducto, dv.Cantidad, dv.PrecioUnitario
                FROM FacturaElectronica f
                INNER JOIN DetalleVenta dv ON f.VentaId = dv.VentaId
                INNER JOIN Producto p ON dv.ProductoId = p.Id
                WHERE f.Id = @IdFactura";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@IdFactura", idFactura);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                productos.Add(new
                {
                    NombreProducto = reader.GetString("NombreProducto"),
                    Cantidad = reader.GetInt32("Cantidad"),
                    PrecioUnitario = reader.GetDecimal("PrecioUnitario")
                });
            }

            return productos;
        }

        private void RegistrarEnvioEmail(int idFactura, string emailDestino, string estado, string mensaje)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand("RegistrarEnvioFactura", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@IdFactura", idFactura);
            command.Parameters.AddWithValue("@TipoEnvio", "Email");
            command.Parameters.AddWithValue("@Destinatario", (object?)emailDestino ?? DBNull.Value);
            command.Parameters.AddWithValue("@EstadoEnvio", estado);
            command.Parameters.AddWithValue("@MensajeRespuesta", (object?)mensaje ?? DBNull.Value);

            command.ExecuteNonQuery();
        }
    }
}
