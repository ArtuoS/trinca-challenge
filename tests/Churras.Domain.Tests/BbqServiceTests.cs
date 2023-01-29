using Churras.Domain.Tests.Builders;
using CrossCutting;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Domain.Services;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Moq;

namespace Churras.Domain.Tests
{
    public class BbqServiceTests
    {
        private BbqService _sut;
        private PersonService _personService;
        private Mock<IBbqRepository> _bbqRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<SnapshotStore> _snapshotStoreMock;
        private Mock<Database> _databaseMock;

        [SetUp]
        public void SetUp()
        {
            _bbqRepositoryMock = new Mock<IBbqRepository>();
            _personRepositoryMock = new Mock<IPersonRepository>();
            _databaseMock = new Mock<Database>();
            _snapshotStoreMock = new Mock<SnapshotStore>(_databaseMock.Object);

            _personService = new PersonService(_personRepositoryMock.Object);
            _sut = new BbqService(_bbqRepositoryMock.Object, _personService, _snapshotStoreMock.Object);
        }

        [Test]
        public async Task CreateNewBbq_GivenCorrectDateAndReason_CreateBbq()
        {
            // Arrange
            _personRepositoryMock.Setup(s => s.GetOrCreateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new PersonBuilder().WithId(Guid.NewGuid().ToString())
                                                            .WithName("Name")
                                                            .IsCoOwner(true)
                                                            .Build()));

            _snapshotStoreMock.Setup(s => s.SingleOrDefaultCollection<Lookups>(It.IsAny<string>()))
               .Returns(Task.FromResult(new Lookups { ModeratorIds = new List<string> { "e78fec8f-8c83-48df-ac60-a48527932b1a" } }));

            Bbq createdBbq = null;
            _bbqRepositoryMock.Setup(s => s.SaveAsync(It.IsAny<Bbq>()))
                .Callback<Bbq>(bbq => createdBbq = bbq);

            Person createdPerson = null;
            _personRepositoryMock.Setup(s => s.SaveAsync(It.IsAny<Person>()))
                .Callback<Person>(person => createdPerson = person);

            // Act
            var result = await _sut.CreateNewBbq(DateTime.Now.AddDays(25), "Test", false);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Message.Should().Be("Barbecue was successfully created!");
            result.Data.Should().Be(createdBbq.TakeSnapshot());

            createdBbq.Status.Should().Be(BbqStatus.New);
            createdBbq.Id.Should().NotBeNull();

            createdPerson.Id.Should().NotBeNull();
        }

        [Test]
        public async Task GetBbqCart_PersonIsModeratorAndBbqAndBbqCartExists_ReturnCart()
        {
            // Arrange
            _personRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new PersonBuilder().WithId(Guid.NewGuid().ToString())
                                                            .WithName("Name")
                                                            .IsCoOwner(true)
                                                            .Build()));

            var bbqCart = new BbqCart(Guid.NewGuid().ToString()) { Meat = 300, Veggies = 300 };
            _bbqRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new BbqBuilder().WithId(Guid.NewGuid().ToString())
                                                         .WithBbqCart(bbqCart)
                                                         .Build()));

            // Act
            var result = await _sut.GetBbqCart(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            // Assert
            result.IsValid.Should().BeTrue();
            result.Message.Should().Be("A cart was successfully fetched.");
            result.Data.Should().Be(bbqCart.TakeSnapshot());
        }

        [Test]
        public async Task GetProposedBbqs_PassingAPersonIdWithInvites_ReturnProposedBbqs()
        {
            // Arrange
            var bbqInviteId = Guid.NewGuid().ToString();
            var invites = new List<Invite>
            {
                new Invite { Id = bbqInviteId, Bbq = bbqInviteId, Date = DateTime.Now.AddDays(25), Status = InviteStatus.Accepted }
            }.AsEnumerable();

            _personRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new PersonBuilder().WithId(Guid.NewGuid().ToString())
                                                            .WithName("Name")
                                                            .WithInvites(invites)
                                                            .Build()));

            var bbq = new BbqBuilder().WithId(bbqInviteId)
                                      .Build();

            _bbqRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(bbq));

            // Act
            var result = await _sut.GetProposedBbqs(Guid.NewGuid().ToString());

            // Assert
            result.IsValid.Should().BeTrue();
            result.Message.Should().Be("Proposed barbecues were successfully fetched.");
            var dataList = result.Data as List<object>;
            dataList[0].Should().Be(bbq.TakeSnapshot());
        }

        [Test]
        public async Task ModerateBbq_BbqGonnaHapped_UpdateBbqStatusToPendingConfirmations()
        {
            // Arrange
            var bbqId = Guid.NewGuid().ToString();
            _bbqRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new BbqBuilder().WithId(bbqId)
                                                         .Build()));

            _personRepositoryMock.Setup(s => s.GetOrCreateAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new PersonBuilder().WithId(Guid.NewGuid().ToString())
                                                            .WithName("Name")
                                                            .IsCoOwner(true)
                                                            .Build()));

            _snapshotStoreMock.Setup(s => s.SingleOrDefaultCollection<Lookups>(It.IsAny<string>()))
                .Returns(Task.FromResult(new Lookups { ModeratorIds = new List<string>(), PeopleIds = new List<string> { "e78fec8f-8c83-48df-ac60-a48527932b1a" } }));

            Bbq createdBbq = null;
            _bbqRepositoryMock.Setup(s => s.SaveAsync(It.IsAny<Bbq>()))
                .Callback<Bbq>(bbq => createdBbq = bbq);

            Person createdPerson = null;
            _personRepositoryMock.Setup(s => s.SaveAsync(It.IsAny<Person>()))
                .Callback<Person>(person => createdPerson = person);

            // Act
            var result = await _sut.ModerateBbq(bbqId, true, false);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Message.Should().Be("Members were successfully invited to barbecue.");
            result.Data.Should().Be(createdBbq.TakeSnapshot());

            createdBbq.Status.Should().Be(BbqStatus.PendingConfirmations);
            createdPerson.Invites.Count().Should().Be(1);
        }

        [Test]
        public async Task ModerateBbq_BbqNotGonnaHapped_UpdateBbqStatusToItsNotGonnaHappen()
        {
            // Arrange
            var bbqId = Guid.NewGuid().ToString();
            _bbqRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new BbqBuilder().WithId(bbqId)
                                                         .Build()));

            Bbq createdBbq = null;
            _bbqRepositoryMock.Setup(s => s.SaveAsync(It.IsAny<Bbq>()))
                .Callback<Bbq>(bbq => createdBbq = bbq);

            // Act
            var result = await _sut.ModerateBbq(bbqId, false, false);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Message.Should().Be("Barbecue was successfully canceled.");
            result.Data.Should().Be(createdBbq.TakeSnapshot());

            createdBbq.Status.Should().Be(BbqStatus.ItsNotGonnaHappen);
        }

        [Test]
        public async Task HandleBbqInvite_PersonAcceptInvite_IncreaseConfirmedPeople()
        {
            // Arrange
            var personId = Guid.NewGuid().ToString();
            var bbqId = Guid.NewGuid().ToString();
            _bbqRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new BbqBuilder().WithId(bbqId)
                                                         .WithStatus(BbqStatus.PendingConfirmations)
                                                         .Build()));

            _personRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new PersonBuilder().WithId(personId)
                                                            .WithName("Name")
                                                            .IsCoOwner(true)
                                                            .Build()));

            Bbq createdBbq = null;
            _bbqRepositoryMock.Setup(s => s.SaveAsync(It.IsAny<Bbq>()))
                .Callback<Bbq>(bbq => createdBbq = bbq);

            // Act
            var result = await _sut.HandleBbqInvite(bbqId, personId, false, BbqInviteType.Accept);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Message.Should().Be("Invite accepted handled.");

            createdBbq.Status.Should().Be(BbqStatus.PendingConfirmations);
            createdBbq.ConfirmedPeople.Count.Should().Be(1);
        }

        [Test]
        public async Task HandleBbqInvite_PersonDeclineInvite_ConfirmedPeopleShouldNotIncrease()
        {
            // Arrange
            var personId = Guid.NewGuid().ToString();
            var bbqId = Guid.NewGuid().ToString();
            _bbqRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new BbqBuilder().WithId(bbqId)
                                                         .WithStatus(BbqStatus.PendingConfirmations)
                                                         .Build()));

            _personRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new PersonBuilder().WithId(personId)
                                                            .WithName("Name")
                                                            .IsCoOwner(true)
                                                            .Build()));

            Bbq createdBbq = null;
            _bbqRepositoryMock.Setup(s => s.SaveAsync(It.IsAny<Bbq>()))
                .Callback<Bbq>(bbq => createdBbq = bbq);

            // Act
            var result = await _sut.HandleBbqInvite(bbqId, personId, false, BbqInviteType.Decline);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Message.Should().Be("Invite accepted handled.");

            createdBbq.Status.Should().Be(BbqStatus.PendingConfirmations);
            createdBbq.ConfirmedPeople.Count.Should().Be(0);
        }
    }
}