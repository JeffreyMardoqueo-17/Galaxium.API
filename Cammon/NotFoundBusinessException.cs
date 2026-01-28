using Microsoft.AspNetCore.Http;

namespace Galaxium.API.Common
{
    public sealed class NotFoundBusinessException : BusinessException
    {
        public NotFoundBusinessException(string message)
            : base(message, StatusCodes.Status404NotFound)
        {
        }
    }
}
