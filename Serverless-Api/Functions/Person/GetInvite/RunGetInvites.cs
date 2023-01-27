using Domain.Entities;
using Domain.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunGetInvites
    {
        private readonly Person _person;
        private readonly IPersonService _personService;
        public RunGetInvites(Person person, IPersonService personService)
        {
            _person = person;
            _personService = personService;
        }

        [Function(nameof(RunGetInvites))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "person/invites")] HttpRequestData req)
        {
            var person = await _personService.GetAsync(_person.Id);
            if (person is null)
                return req.CreateResponse(System.Net.HttpStatusCode.NoContent);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}