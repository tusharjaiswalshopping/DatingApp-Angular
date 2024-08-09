using System.Security.Cryptography.X509Certificates;

namespace API
{
    public class ApiException
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string? Details { get; set; }

        public ApiException(int statusCode, string message, string? details = null)
        {
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }
    }
}
