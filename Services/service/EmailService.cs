using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Galaxium.Api.Services.Interfaces;

namespace Galaxium.Api.Services.service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarEmail(string emailReceptor, string assunto, string cuerpo)
        {
          var emailEmisor = _configuration.GetValue<string>("CONFIGURACIONES:EMAIL");
var emailPassword = _configuration.GetValue<string>("CONFIGURACIONES:PASSWORD");
var smtpHost = _configuration.GetValue<string>("CONFIGURACIONES:HOST");
var smtpPort = _configuration.GetValue<int>("CONFIGURACIONES:PUERTO");

            var smtCliente = new SmtpClient(smtpHost, smtpPort);
            smtCliente.EnableSsl = true;
            smtCliente.UseDefaultCredentials = false;

            smtCliente.Credentials = new NetworkCredential(emailEmisor, emailPassword);
            var mensaje = new MailMessage(emailEmisor!, emailReceptor, assunto, cuerpo);
            await smtCliente.SendMailAsync(mensaje);
        }
    }
}