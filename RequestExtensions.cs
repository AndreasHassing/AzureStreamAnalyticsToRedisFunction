using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AzureStreamAnalyticsToRedisFunction
{
    internal static class RequestExtensions
    {
        /// <summary>
        /// Read the body of the request as a string.
        /// </summary>
        public static async Task<string> ReadRequestBodyAsStringAsync(this HttpRequest request)
        {
            using var reader = new StreamReader(request.Body);

            return await reader.ReadToEndAsync();
        }
    }
}