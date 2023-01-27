using CrossCutting;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using Domain.Repositories;
using Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Services
{
    internal class BbqService : IBbqService
    {
        private readonly IBbqRepository _bbqRepository;
        private readonly IPersonService _personService;
        private readonly SnapshotStore _snapshots;

        public BbqService(IBbqRepository bbqRepository, IPersonService personService, SnapshotStore snapshots)
        {
            _bbqRepository = bbqRepository;
            _personService = personService;
            _snapshots = snapshots;
        }

        public async Task<Bbq> GetAsync(string id) => await _bbqRepository.GetAsync(id);

        /// <summary>
        /// Create a new barbecue.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="reason"></param>
        /// <param name="isTrincaPaying"></param>
        /// <returns></returns>
        public async Task<Response> CreateNewBbq(DateTime date, string reason, bool isTrincaPaying)
        {
            var bbq = new Bbq();
            bbq.Apply(new ThereIsSomeoneElseInTheMood(Guid.NewGuid(), date, reason, isTrincaPaying));

            await _bbqRepository.SaveAsync(bbq);

            var lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();
            foreach (var personId in lookups.ModeratorIds)
            {
                await _personService.InvitePersonToBbq(personId, bbq);
            }

            return new Response("Barbecue was successfully created!", true, bbq.TakeSnapshot());
        }

        /// <summary>
        /// Get a barbecue cart.
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="bbqId"></param>
        /// <returns></returns>
        public async Task<Response> GetBbqCart(string personId, string bbqId)
        {
            var isModerator = await _personService.IsModerator(personId);
            if (!isModerator)
                return new Response("Person is not a moderator.", false);

            var bbq = await _bbqRepository.GetAsync(bbqId);
            if (bbq is null)
                return new Response($"Barbecue with Id {bbqId} couldn't be found.", false);

            if (bbq.BbqCart is null)
                return new Response($"Cart for the barbecue with Id {bbqId} couldn't be found.", false);

            return new Response("A cart was successfully fetched.", true, bbq.BbqCart.TakeSnapshot());
        }

        /// <summary>
        /// Get all proposed barbecues by person.
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public async Task<Response> GetProposedBbqs(string personId)
        {
            var snapshots = new List<object>();
            var person = await _personService.GetAsync(personId);
            if (person is null)
                return new Response($"User with Id {personId} was not found.", false);

            var bbqInvites = person.Invites.Where(i => i.Date > DateTime.Now && i.Status != InviteStatus.Declined)
                                           .Select(o => o.Id)
                                           .ToList();

            foreach (var bbqId in bbqInvites)
            {
                var bbq = await _bbqRepository.GetAsync(bbqId);
                if (bbq is null || bbq.Status == BbqStatus.ItsNotGonnaHappen)
                    continue;

                snapshots.Add(bbq.TakeSnapshot());
            }

            return new Response("Proposed barbecues were successfully fetched.", true, snapshots);
        }

        /// <summary>
        /// Moderate a barbecue. Here a barbecue can be rejected or approved.
        /// </summary>
        /// <param name="bbqId"></param>
        /// <param name="gonnaHappen"></param>
        /// <param name="isTrincaPaying"></param>
        /// <returns></returns>
        public async Task<Response> ModerateBbq(string bbqId, bool gonnaHappen, bool isTrincaPaying)
        {
            var bbq = await _bbqRepository.GetAsync(bbqId);

            if (bbq is null)
                return new Response($"Barbecue with Id {bbqId} couldn't be found.", false);

            bbq.Apply(new BbqStatusUpdated(gonnaHappen, isTrincaPaying));

            if (gonnaHappen)
            {
                var lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();
                foreach (var personId in lookups.PeopleIds.Except(lookups.ModeratorIds))
                {
                    await _personService.InvitePersonToBbq(personId, bbq);
                }
            }

            await _bbqRepository.SaveAsync(bbq);

            return new Response($"Members were successfully invited to barbecue.", true, bbq.TakeSnapshot());
        }

        /// <summary>
        /// Handle a event invite. It can be a decline or accept event.
        /// </summary>
        /// <param name="inviteId"></param>
        /// <param name="personId"></param>
        /// <param name="isVeg"></param>
        /// <param name="bbqInviteType"></param>
        /// <returns></returns>
        public async Task<Response> HandleBbqInvite(string inviteId, string personId, bool isVeg, BbqInviteType bbqInviteType)
        {
            var bbq = await _bbqRepository.GetAsync(inviteId);
            if (bbq is null || bbq.Status != BbqStatus.PendingConfirmations)
                return new Response($"Barbecue with Id {inviteId} couldn't be found.", false);

            var @event = GetInviteEvent(inviteId, personId, isVeg, bbqInviteType);
            var response = await _personService.HandleBbqInvite(personId, @event);
            if (!response.IsValid)
                return new Response(response.Message, false);

            bbq.Apply(@event);
            await _bbqRepository.SaveAsync(bbq);

            return new Response(response.Message, true, response.Data);
        }

        /// <summary>
        /// Factory method that creates a accept or decline event.
        /// </summary>
        /// <param name="inviteId"></param>
        /// <param name="personId"></param>
        /// <param name="isVeg"></param>
        /// <param name="bbqInviteType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private IEvent GetInviteEvent(string inviteId, string personId, bool isVeg, BbqInviteType bbqInviteType)
        {
            switch (bbqInviteType)
            {
                case BbqInviteType.Accept:
                    return new InviteWasAccepted(personId, inviteId, isVeg);

                case BbqInviteType.Decline:
                    return new InviteWasDeclined(personId, inviteId, isVeg);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}