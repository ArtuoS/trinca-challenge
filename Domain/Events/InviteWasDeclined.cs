namespace Domain.Events
{
    public class InviteWasDeclined : IEvent
    {
        public InviteWasDeclined(string personId, string inviteId, bool isVeg)
        {
            InviteId = inviteId;
            PersonId = personId;
            IsVeg = isVeg;
        }

        public InviteWasDeclined()
        { }

        public string InviteId { get; set; }
        public string PersonId { get; set; }
        public bool IsVeg { get; set; }
    }
}