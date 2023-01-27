using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System.Net;

namespace System
{
    public static class HttpRequestDataExtensions
    {
        public static async Task<HttpResponseData> CreateResponse(this HttpRequestData req, HttpStatusCode statusCode, object? body)
        {
            var response = req.CreateResponse(statusCode);

            if (body != null)
            {
                await response.WriteAsJsonAsync(body);
            }
            return response;
        }

        public static async Task<T?> Body<T>(this HttpRequestData request)
        {
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
                return default;

            return JsonConvert.DeserializeObject<T>(requestBody);
        }
    }
}