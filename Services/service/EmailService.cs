using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Galaxium.Api.Services.Interfaces;

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
}
