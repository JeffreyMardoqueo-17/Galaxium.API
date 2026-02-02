using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.Api.Services.Interfaces
{
    public interface IEmailService
    {
        Task EnviarEmail(string emailReceptor, string assunto, string cuerpo);
    }
}