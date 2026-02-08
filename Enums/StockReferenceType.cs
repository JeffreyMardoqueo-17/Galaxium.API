using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galaxium.Api.Enums
{
   public enum StockReferenceType
    {
        Purchase = 1,     // entra stock
        Sale = 2,         // sale stock
        Adjustment = 3    // corrección manual
    }
}