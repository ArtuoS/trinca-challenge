using Domain.Common;
using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using Domain.Services.Interfaces;
using System.Threading.Tasks;

namespace Domain.Services
{
    /// Deixei public para realizar os testes, sem os testes poderia ser trocado para internal para não ser possível acessar de fora do Domain.
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;
        public PersonService(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<Person> GetAsync(string id) => await _personRepository.GetAsync(id);

        /// <summary>
        /// Create an invite to a specific person.
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="bbq"></param>
        /// <returns></returns>
        public async Task InvitePersonToBbq(string personId, Bbq bbq)
        {
            var person = await _personRepository.GetOrCreateAsync(personId);
            person.Apply(new PersonHasBeenInvitedToBbq(bbq.Id, bbq.Date, bbq.Reason));

            await _personRepository.SaveAsync(person);
        }

        /// <summary>
        /// Validate if a person is a moderator.
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public async Task<bool> IsModerator(string personId)
        {
            var person = await _personRepository.GetAsync(personId);
            return person != null && person.IsCoOwner;
        }

        /// <summary>
        /// Handle a event invite. It can be a decline or accept event.
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<Response> HandleBbqInvite(string personId, IEvent @event)
        {
            var person = await _personRepository.GetAsync(personId);
            if (person is null)
                return new Response($"Person with Id {personId} couldn't be found.", false);

            person.Apply(@event);
            await _personRepository.SaveAsync(person);

            return new Response($"Invite accepted handled.", true, person.TakeSnapshot());
        }
    }
}