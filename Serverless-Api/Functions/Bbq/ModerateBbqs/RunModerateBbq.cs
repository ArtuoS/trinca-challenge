using Domain.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunModerateBbq
    {
        private readonly IBbqService _bbqService;

        public RunModerateBbq(IBbqService bbqService)
        {
            _bbqService = bbqService;
        }

        [Function(nameof(RunModerateBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "churras/{id}/moderar")] HttpRequestData req, string id)
        {
            var moderationRequest = await req.Body<ModerateBbqRequest>() ?? throw new ArgumentNullException(nameof(HttpRequestData));

            var response = await _bbqService.ModerateBbq(id, moderationRequest.GonnaHappen, moderationRequest.TrincaWillPay);

            if (response.IsValid)
                return await req.CreateResponse(System.Net.HttpStatusCode.OK, response.Data);

            return await req.CreateResponse(System.Net.HttpStatusCode.BadRequest, response.Message);
        }
    }
}