using Domain.Entities;
using System;

namespace Domain.Repositories
{
    internal class PersonRepository : StreamRepository<Person>, IPersonRepository
    {
        public PersonRepository(IEventStore<Person> eventStore)
            : base(eventStore)
        { }
    }
}