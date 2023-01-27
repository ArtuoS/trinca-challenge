using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IBbqService : IBaseService<Bbq>
    {
        Task<Response> CreateNewBbq(DateTime date, string reason, bool isTrincaPaying);

        Task<Response> ModerateBbq(string bbqId, bool gonnaHappen, bool isTrincaPaying);

        Task<Response> GetBbqCart(string personId, string bbqId);

        Task<Response> GetProposedBbqs(string personId);

        Task<Response> HandleBbqInvite(string inviteId, string personId, bool isVeg, BbqInviteType bbqInviteType);
    }
}