using Domain.Entities;
using Domain.Enums;
using Domain.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using static Serverless_Api.RunAcceptInvite;

namespace Serverless_Api
{
    public partial class RunDeclineInvite
    {
        private readonly Person _person;
        private readonly IBbqService _bbqService;

        public RunDeclineInvite(Person person, IBbqService bbqService)
        {
            _person = person;
            _bbqService = bbqService;
        }

        [Function(nameof(RunDeclineInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/decline")] HttpRequestData req, string inviteId)
        {
            var answer = await req.Body<InviteAnswer>() ?? throw new ArgumentNullException(nameof(HttpRequestData));

            var response = await _bbqService.HandleBbqInvite(inviteId, _person.Id, answer.IsVeg, BbqInviteType.Decline);
            if (response.IsValid)
                return await req.CreateResponse(System.Net.HttpStatusCode.OK, response.Data);

            return await req.CreateResponse(System.Net.HttpStatusCode.BadRequest, response.Data);
        }
    }
}