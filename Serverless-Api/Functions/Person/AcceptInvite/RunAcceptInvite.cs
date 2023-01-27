using Domain.Entities;
using Domain.Enums;
using Domain.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunAcceptInvite
    {
        private readonly Person _person;
        private readonly IBbqService _bbqService;

        public RunAcceptInvite(Person person, IBbqService bbqService)
        {
            _person = person;
            _bbqService = bbqService;
        }

        // Os invites estavam podendo ser aceitos mesmo com os barbecues estando com status diferentes de PendingConfirmations
        [Function(nameof(RunAcceptInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/accept")] HttpRequestData req, string inviteId)
        {
            var answer = await req.Body<InviteAnswer>() ?? throw new ArgumentNullException(nameof(HttpRequestData));

            var response = await _bbqService.HandleBbqInvite(inviteId, _person.Id, answer.IsVeg, BbqInviteType.Accept);
            if (response.IsValid)
                return await req.CreateResponse(System.Net.HttpStatusCode.OK, response.Data);

            return await req.CreateResponse(System.Net.HttpStatusCode.BadRequest, response.Message);
        }
    }
}