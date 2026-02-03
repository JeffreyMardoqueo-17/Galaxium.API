using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.Api.Services.Interfaces
{

    public interface IEmailService
    {
        Task EnviarEmailRegistroCliente(string? emailReceptor, string nombreCliente);
        Task EnviarEmailFactura(string? emailReceptor, string nombreCliente, byte[] facturaPdfBytes, string nombreArchivoPdf);
    }

}