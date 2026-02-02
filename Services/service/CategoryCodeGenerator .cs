using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.Api.Services.service
{
   using System.Text;
    using Galaxium.Api.Services.Interfaces;
    using Galaxium.API.Common;

    public class CategoryCodeGenerator : ICategoryCodeGenerator
{
    public Task<string> GenerateAsync(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new BusinessException("Category name is required to generate code.");

        // Tomar iniciales
        var words = categoryName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var code = string.Join("", words
            .Take(3)
            .Select(w => char.ToUpper(w[0])));

        return Task.FromResult(code);
    }
}

} 