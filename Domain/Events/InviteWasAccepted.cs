namespace Domain.Events
{
    public class InviteWasAccepted : IEvent
    {
        public InviteWasAccepted(string personId, string inviteId, bool isVeg)
        {
            PersonId = personId;
            InviteId = inviteId;
            IsVeg = isVeg;
        }

        public InviteWasAccepted()
        { }

        public string PersonId { get; set; }
        public string InviteId { get; set; }
        public bool IsVeg { get; set; }
    }
}