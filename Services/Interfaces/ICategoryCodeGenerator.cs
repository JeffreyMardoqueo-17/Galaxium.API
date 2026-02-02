using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.Api.Services.Interfaces
{
    public interface ICategoryCodeGenerator
    {
        Task<string> GenerateAsync(string categoryName);
    }

}