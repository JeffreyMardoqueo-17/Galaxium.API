using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galaxium.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Galaxium.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("registro")]
        public async Task<IActionResult> RegistrarCliente(string email, string nombre)
        {
            await _emailService.EnviarEmailRegistroCliente(email, nombre);
            return Ok(new { mensaje = "Correo de registro enviado." });
        }

        [HttpPost("factura")]
        public async Task<IActionResult> EnviarFactura(string email, string nombre, IFormFile facturaPdf)
        {
            using var ms = new MemoryStream();
            await facturaPdf.CopyToAsync(ms);
            var pdfBytes = ms.ToArray();

            await _emailService.EnviarEmailFactura(email, nombre, pdfBytes, facturaPdf.FileName);

            return Ok(new { mensaje = "Factura enviada exitosamente." });
        }

    }
}