using Domain.Entities;

namespace Churras.Domain.Tests.Builders
{
    internal class PersonBuilder
    {
        private string? _name;
        private string? _id;
        private bool? _isCoOwner;
        private IEnumerable<Invite> _invites;

        public PersonBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public PersonBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public PersonBuilder IsCoOwner(bool isCoOwner)
        {
            _isCoOwner = isCoOwner;
            return this;
        }

        public PersonBuilder WithInvites(IEnumerable<Invite> invites)
        {
            _invites = invites;
            return this;
        }

        public Person Build()
        {
            return new Person
            {
                Id = _id ?? string.Empty,
                Name = _name ?? string.Empty,
                IsCoOwner = _isCoOwner ?? false,
                Invites = _invites ?? new List<Invite>()
            };
        }
    }
}