namespace Galaxium.API.Common
{
    /// <summary>
    /// Base exception for controlled business rule violations.
    /// </summary>
    public class BusinessException : Exception
    {
        public int StatusCode { get; }

        protected BusinessException(string message, int statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public BusinessException(string message)
            : this(message, StatusCodes.Status400BadRequest)
        {
        }
    }
}
