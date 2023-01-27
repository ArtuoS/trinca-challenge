using Domain.Common;
using Domain.Entities;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IPersonService : IBaseService<Person>
    {
        Task InvitePersonToBbq(string personId, Bbq bbq);

        Task<bool> IsModerator(string personId);

        Task<Response> HandleBbqInvite(string personId, IEvent @event);
    }
}