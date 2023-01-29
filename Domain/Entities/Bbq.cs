using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entities
{
    public class Bbq : AggregateRoot
    {
        public string Reason { get; set; }
        public BbqStatus Status { get; set; }
        public DateTime Date { get; set; }
        public bool IsTrincasPaying { get; set; }
        public List<string> ConfirmedPeople { get; set; } = new List<string>();
        public BbqCart? BbqCart { get; set; }
        private int _confirmations => ConfirmedPeople.Count;

        internal void When(ThereIsSomeoneElseInTheMood @event)
        {
            Id = @event.Id.ToString();
            Date = @event.Date;
            Reason = @event.Reason;
            Status = BbqStatus.New;
        }

        internal void When(BbqStatusUpdated @event)
        {
            if (@event.GonnaHappen)
                Status = BbqStatus.PendingConfirmations;
            else
                Status = BbqStatus.ItsNotGonnaHappen;

            if (@event.TrincaWillPay)
                IsTrincasPaying = true;
        }

        internal void When(InviteWasDeclined @event)
        {
            if (BbqCart is null)
                BbqCart = new BbqCart(@event.InviteId);

            if (ConfirmedPeople.Any(w => w.Equals(@event.PersonId)))
            {
                ConfirmedPeople.Remove(@event.PersonId);
                BbqCart.DecreaseQuantitiesByIsVeg(@event.IsVeg);
            }

            if (_confirmations < 7)
                Status = BbqStatus.PendingConfirmations;
        }

        internal void When(InviteWasAccepted @event)
        {
            if (BbqCart is null)
                BbqCart = new BbqCart(@event.InviteId);

            BbqCart.IncreaseQuantitiesByIsVeg(@event.IsVeg);
            ConfirmedPeople.Add(@event.PersonId);

            if (_confirmations >= 7)
                Status = BbqStatus.Confirmed;
        }

        public object TakeSnapshot()
        {
            return new
            {
                Id,
                Date,
                IsTrincasPaying,
                Status = Status.ToString()
            };
        }
    }
}