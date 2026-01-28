using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.API.Entities
{
    /// <summary>
    /// Configuración para la generación y validación de JWT.
    /// </summary>
    public  class JwtOptions
    {
        public string Key { get; init; } = string.Empty;
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public int AccessTokenMinutes { get; init; }
        public int RefreshTokenDays { get; init; }
    }

}