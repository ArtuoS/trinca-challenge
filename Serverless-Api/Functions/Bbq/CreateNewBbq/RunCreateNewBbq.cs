using Domain.Entities;
using Domain.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace Serverless_Api
{
    public partial class RunCreateNewBbq
    {
        private readonly Person _person;
        private readonly IBbqService _bbqService;

        public RunCreateNewBbq(IBbqService bbqService, Person person)
        {
            _bbqService = bbqService;
            _person = person;
        }

        [Function(nameof(RunCreateNewBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "churras")] HttpRequestData request)
        {
            var input = await request.Body<NewBbqRequest>() ?? throw new ArgumentNullException(nameof(HttpRequestData));

            var response = await _bbqService.CreateNewBbq(input.Date, input.Reason, input.IsTrincasPaying);

            if (response.IsValid)
                return await request.CreateResponse(HttpStatusCode.Created, response.Data);

            return await request.CreateResponse(HttpStatusCode.BadRequest, response.Message);
        }
    }
}