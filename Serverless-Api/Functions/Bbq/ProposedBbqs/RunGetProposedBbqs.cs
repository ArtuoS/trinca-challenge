using Domain.Entities;
using Domain.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace Serverless_Api
{
    public partial class RunGetProposedBbqs
    {
        private readonly Person _person;
        private readonly IBbqService _bbqService;

        public RunGetProposedBbqs(Person person, IBbqService bbqService)
        {
            _person = person;
            _bbqService = bbqService;
        }

        [Function(nameof(RunGetProposedBbqs))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras")] HttpRequestData req)
        {
            var response = await _bbqService.GetProposedBbqs(_person.Id);

            if (response.IsValid)
                return await req.CreateResponse(HttpStatusCode.OK, response.Data);

            return await req.CreateResponse(HttpStatusCode.BadRequest, response.Message);
        }
    }
}