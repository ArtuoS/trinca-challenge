using Domain.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;

namespace Serverless_Api.Middlewares
{
    internal class AuthMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var loggedPerson = context.InstanceServices.GetService<Person>();

            context.BindingContext.BindingData.TryGetValue("headers", out object headers);

            if (headers == null)
            {
                context.InvokeResult(context.GetHttpRequestData().CreateResponse(HttpStatusCode.Unauthorized));
                return;
            }

            loggedPerson.Id = JsonConvert.DeserializeObject<Headers>(headers.ToString()).PersonId;

            await next(context);
        }

        public class Headers
        {
            public string PersonId { get; set; }
        }
    }

    public static class FunctionContextExtensions
    {
        internal static HttpRequestData GetHttpRequestData(this FunctionContext context)
        {
            var keyValuePair = context.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature");
            var functionBindingsFeature = keyValuePair.Value;
            var type = functionBindingsFeature.GetType();
            var inputData = type.GetProperties().Single(p => p.Name == "InputData").GetValue(functionBindingsFeature) as IReadOnlyDictionary<string, object>;
            return inputData?.Values.SingleOrDefault(o => o is HttpRequestData) as HttpRequestData;
        }

        internal static void InvokeResult(this FunctionContext context, HttpResponseData response)
        {
            var keyValuePair = context.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature");
            var functionBindingsFeature = keyValuePair.Value;
            var type = functionBindingsFeature.GetType();
            var result = type.GetProperties().Single(p => p.Name == "InvocationResult");
            result.SetValue(functionBindingsFeature, response);
        }
    }
}