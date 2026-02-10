using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Galaxium.Api.Services.Interfaces;
using Galaxium.API.Entities;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string? _emailEmisor;
    private readonly string? _emailPassword;
    private readonly string? _smtpHost;
    private readonly int _smtpPort;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _emailEmisor = _configuration.GetValue<string>("CONFIGURACIONES:EMAIL");
        _emailPassword = _configuration.GetValue<string>("CONFIGURACIONES:PASSWORD");
        _smtpHost = _configuration.GetValue<string>("CONFIGURACIONES:HOST");
        _smtpPort = _configuration.GetValue<int>("CONFIGURACIONES:PUERTO");
    }

    private SmtpClient CrearClienteSmtp()
    {
        return new SmtpClient(_smtpHost, _smtpPort)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_emailEmisor, _emailPassword)
        };
    }

    // Método para enviar correo de registro cliente (HTML simple y limpio)
    public async Task EnviarEmailRegistroCliente(string emailReceptor, string nombreCliente)
    {
        var asunto = "Bienvenido a Galaxium - Gracias por registrarte";
        var cuerpoHtml = $@"
<html>
  <body style='font-family: Arial, sans-serif; color: #222; background-color: #f9f9f9; padding: 20px;'>
    <div style='max-width: 600px; margin: auto; background: white; border-radius: 8px; padding: 30px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
      <h1 style='color: #007acc;'>¡Hola {nombreCliente}!</h1>
      <p>Gracias por registrarte en nuestra tienda. Es un gusto tenerte como cliente.</p>
      <p>Esperamos que encuentres justo lo que buscas y vuelvas pronto por más.</p>
      <p style='margin-top: 30px;'>Saludos cordiales,<br/>Tu tienda de confianza</p>
    </div>
  </body>
</html>";

        await EnviarEmailHtml(emailReceptor, asunto, cuerpoHtml);
    }

    // Método para enviar correo con factura (adjuntando PDF)
    public async Task EnviarEmailFactura(string emailReceptor, string nombreCliente, byte[] facturaPdfBytes, string nombreArchivoPdf)
    {
        var asunto = "Tu factura de compra en Galaxium";
        var cuerpoHtml = $@"
<html>
  <body style='font-family: Arial, sans-serif; color: #222; background-color: #f9f9f9; padding: 20px;'>
    <div style='max-width: 600px; margin: auto; background: white; border-radius: 8px; padding: 30px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
      <h1 style='color: #007acc;'>Hola {nombreCliente},</h1>
      <p>Gracias por tu compra. Adjuntamos la factura para tu referencia.</p>
      <p>Esperamos verte de nuevo pronto para ofrecerte más productos que te gusten.</p>
      <p style='margin-top: 30px;'>Saludos cordiales,<br/>Tu tienda de confianza</p>
    </div>
  </body>
</html>";

        using var cliente = CrearClienteSmtp();
        using var mensaje = new MailMessage()
        {
            From = new MailAddress(_emailEmisor),
            Subject = asunto,
            IsBodyHtml = true,
            Body = cuerpoHtml
        };

        mensaje.To.Add(emailReceptor);

        // Adjuntar PDF desde arreglo de bytes
        using var streamPdf = new System.IO.MemoryStream(facturaPdfBytes);
        var attachment = new Attachment(streamPdf, nombreArchivoPdf, "application/pdf");
        mensaje.Attachments.Add(attachment);

        await cliente.SendMailAsync(mensaje);
    }

    // Método general para enviar HTML (sin adjuntos)
    private async Task EnviarEmailHtml(string emailReceptor, string asunto, string cuerpoHtml)
    {
        using var cliente = CrearClienteSmtp();
        using var mensaje = new MailMessage()
        {
            From = new MailAddress(_emailEmisor),
            Subject = asunto,
            Body = cuerpoHtml,
            IsBodyHtml = true
        };

        mensaje.To.Add(emailReceptor);

        await cliente.SendMailAsync(mensaje);
    }

    // Método para enviar correo con detalles de compra bienvenida
    public async Task EnviarEmailCompraBienvenida(
        string? emailReceptor,
        string nombreCliente,
        Sale sale,
        IEnumerable<SaleDetail> detallesVenta,
        string nombreVendedor
    )
    {
        if (string.IsNullOrEmpty(emailReceptor))
            throw new ArgumentException("El email del receptor no puede estar vacío.");

        // Construir tabla de productos
        var filasProductos = new System.Text.StringBuilder();
        decimal totalProductos = 0;

        foreach (var detalle in detallesVenta)
        {
            var nombreProducto = detalle.Product?.Name ?? $"Producto (Id: {detalle.ProductId})";
            var subtotal = detalle.Quantity * detalle.UnitPrice;
            totalProductos += subtotal;

            filasProductos.Append($@"
            <tr>
              <td style='padding: 10px; border-bottom: 1px solid #eee;'>{nombreProducto}</td>
              <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: center;'>{detalle.Quantity}</td>
              <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>${detalle.UnitPrice:F2}</td>
              <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>${subtotal:F2}</td>
            </tr>");
        }

        var asunto = $"Gracias por tu compra - Factura {sale.InvoiceNumber}";
        var cuerpoHtml = $@"
<html>
  <head>
    <meta charset='UTF-8'>
  </head>
  <body style='font-family: Arial, sans-serif; color: #333; background-color: #f9f9f9; padding: 20px;'>
    <div style='max-width: 700px; margin: auto; background: white; border-radius: 8px; padding: 30px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
      
      <!-- Encabezado -->
      <h1 style='color: #007acc; text-align: center; margin-bottom: 30px;'>¡Gracias por tu Compra!</h1>
      <p style='font-size: 16px;'>Hola <strong>{nombreCliente}</strong>,</p>
      <p>Nos complace confirmar que tu compra ha sido procesada exitosamente. Aquí están los detalles de tu pedido:</p>

      <!-- Información de Factura -->
      <div style='background-color: #f0f8ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
        <p style='margin: 5px 0;'><strong>Número de Factura:</strong> {sale.InvoiceNumber}</p>
        <p style='margin: 5px 0;'><strong>Fecha:</strong> {sale.SaleDate:dd/MM/yyyy HH:mm}</p>
        <p style='margin: 5px 0;'><strong>Atendido por:</strong> {nombreVendedor}</p>
      </div>

      <!-- Tabla de Productos -->
      <h3 style='color: #007acc; margin-top: 25px; margin-bottom: 15px;'>Detalle de Compra</h3>
      <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
        <thead>
          <tr style='background-color: #007acc;'>
            <th style='padding: 10px; text-align: left; color: white;'>Producto</th>
            <th style='padding: 10px; text-align: center; color: white;'>Cantidad</th>
            <th style='padding: 10px; text-align: right; color: white;'>Precio Unit.</th>
            <th style='padding: 10px; text-align: right; color: white;'>SubTotal</th>
          </tr>
        </thead>
        <tbody>
          {filasProductos}
        </tbody>
      </table>

      <!-- Resumen de Totales -->
      <div style='border-top: 2px solid #007acc; padding-top: 15px; margin-bottom: 20px;'>
        <div style='display: flex; justify-content: space-between; margin-bottom: 10px;'>
          <span><strong>Subtotal:</strong></span>
          <span>${sale.SubTotal:F2}</span>
        </div>
        {(sale.Discount > 0 ? $@"
        <div style='display: flex; justify-content: space-between; margin-bottom: 10px;'>
          <span><strong>Descuento:</strong></span>
          <span style='color: #ff6b6b;'>-${sale.Discount:F2}</span>
        </div>" : "")}
        <div style='display: flex; justify-content: space-between; margin-bottom: 10px; font-size: 18px; color: #007acc;'>
          <strong>Total:</strong>
          <strong>${sale.Total:F2}</strong>
        </div>
        <div style='display: flex; justify-content: space-between; font-size: 14px; color: #666;'>
          <span><strong>Método de Pago:</strong></span>
          <span>{sale.PaymentMethod?.Name ?? "N/A"}</span>
        </div>
      </div>

      <!-- Mensaje de Cierre -->
      <p style='margin-top: 30px; text-align: center; color: #666;'>
        Gracias por confiar en <strong>Galaxium</strong>. Si tienes alguna pregunta sobre tu pedido, no dudes en contactarnos.
      </p>
      <p style='text-align: center; color: #666;'>Esperamos verte de nuevo pronto.</p>
      <p style='text-align: center; margin-top: 20px; font-size: 12px; color: #999;'>---<br/>Tu tienda de confianza</p>
    </div>
  </body>
</html>";

        await EnviarEmailHtml(emailReceptor, asunto, cuerpoHtml);
    }
}
