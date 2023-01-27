using Domain.Entities;
using Domain.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace Serverless_Api.Functions.Bbq.GetBbqCart
{
    public partial class RunGetBbqCart
    {
        private readonly Person _person;
        private readonly IBbqService _bbqService;

        public RunGetBbqCart(Person person, IBbqService bbqService)
        {
            _person = person;
            _bbqService = bbqService;
        }

        [Function(nameof(RunGetBbqCart))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras/{bbqId}/cart")] HttpRequestData req, string bbqId)
        {
            var response = await _bbqService.GetBbqCart(_person.Id, bbqId);

            if (response.IsValid)
                return await req.CreateResponse(HttpStatusCode.Created, response.Data);

            return await req.CreateResponse(HttpStatusCode.BadRequest, response.Message);
        }
    }
}