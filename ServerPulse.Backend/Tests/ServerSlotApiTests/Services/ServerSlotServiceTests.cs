using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using ServerSlotApi.Data;
using ServerSlotApi.Domain.Entities;
using ServerSlotApi.Services;
using Shared.Repositories;

namespace ServerSlotApiTests.Services
{
    [TestFixture]
    internal class ServerSlotServiceTests
    {
        protected MockRepository mockRepository;
        private Mock<IConfiguration> configurationMock;
        private Mock<IDatabaseRepository<ServerDataDbContext>> repositoryMock;
        private ServerSlotService serverSlotService;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            configurationMock = new Mock<IConfiguration>();
            repositoryMock = new Mock<IDatabaseRepository<ServerDataDbContext>>();
            serverSlotService = new ServerSlotService(configurationMock.Object, repositoryMock.Object);
            cancellationToken = new CancellationToken();
        }
        private Mock<ServerDataDbContext> CreateMockDbContext()
        {
            var options = new DbContextOptionsBuilder<ServerDataDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var mockDbContext = mockRepository.Create<ServerDataDbContext>(options);
            return mockDbContext;
        }
        private static Mock<DbSet<T>> GetDbSetMock<T>(IQueryable<T> data) where T : class
        {
            return data.BuildMockDbSet();
        }
        [Test]
        public async Task GetServerSlotsByEmailAsync_ValidEmail_ReturnsServerSlots()
        {
            // Arrange
            var email = "test@example.com";
            var serverSlots = new List<ServerSlot>
            {
                new ServerSlot { UserEmail = email, Name = "Slot1", CreationDate = DateTime.UtcNow },
                new ServerSlot { UserEmail = email, Name = "Slot2", CreationDate = DateTime.UtcNow }
            };
            var dbContextMock = CreateMockDbContext();
            var dbSetMock = GetDbSetMock(serverSlots.AsQueryable());
            dbContextMock.Setup(x => x.ServerSlots).Returns(dbSetMock.Object);
            repositoryMock.Setup(x => x.CreateDbContextAsync(cancellationToken)).ReturnsAsync(dbContextMock.Object);
            // Act
            var result = await serverSlotService.GetServerSlotsByEmailAsync(email, cancellationToken);
            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Name, Is.EqualTo("Slot2"));
        }
        [Test]
        public async Task GerServerSlotsContainingStringAsync_ValidEmailAndString_ReturnsServerSlots()
        {
            // Arrange
            var email = "test@example.com";
            var searchString = "Slot";
            var serverSlots = new List<ServerSlot>
            {
                new ServerSlot { UserEmail = email, Name = "Slot1", CreationDate = DateTime.UtcNow },
                new ServerSlot { UserEmail = email, Name = "Slot2", CreationDate = DateTime.UtcNow },
                new ServerSlot { UserEmail = email, Name = "Another", CreationDate = DateTime.UtcNow }
            };
            var dbContextMock = CreateMockDbContext();
            var dbSetMock = GetDbSetMock(serverSlots.AsQueryable());
            dbContextMock.Setup(x => x.ServerSlots).Returns(dbSetMock.Object);
            repositoryMock.Setup(x => x.CreateDbContextAsync(cancellationToken)).ReturnsAsync(dbContextMock.Object);
            // Act
            var result = await serverSlotService.GerServerSlotsContainingStringAsync(email, searchString, cancellationToken);
            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.IsTrue(result.All(x => x.Name.Contains(searchString)));
        }
        [Test]
        public async Task CreateServerSlotAsync_ValidSlot_SlotCreated()
        {
            // Arrange
            var serverSlot = new ServerSlot { UserEmail = "test@example.com", Name = "NewSlot", CreationDate = DateTime.UtcNow };
            var dbContextMock = CreateMockDbContext();
            var dbSetMock = GetDbSetMock(new List<ServerSlot>().AsQueryable());
            dbContextMock.Setup(x => x.ServerSlots).Returns(dbSetMock.Object);
            repositoryMock.Setup(x => x.CreateDbContextAsync(cancellationToken)).ReturnsAsync(dbContextMock.Object);
            configurationMock.Setup(c => c["SlotsPerUser"]).Returns("5");
            // Act
            var result = await serverSlotService.CreateServerSlotAsync(serverSlot, cancellationToken);
            // Assert
            dbSetMock.Verify(x => x.AddAsync(serverSlot, cancellationToken), Times.Once);
            dbContextMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
            Assert.That(result, Is.EqualTo(serverSlot));
        }
        [Test]
        public void CreateServerSlotAsync_TooManySlots_ThrowsInvalidOperationException()
        {
            // Arrange
            var serverSlot = new ServerSlot { UserEmail = "test@example.com", Name = "NewSlot", CreationDate = DateTime.UtcNow };
            var dbContextMock = CreateMockDbContext();
            var dbSetMock = GetDbSetMock(new List<ServerSlot> { serverSlot, serverSlot, serverSlot, serverSlot, serverSlot, serverSlot }.AsQueryable());
            dbContextMock.Setup(x => x.ServerSlots).Returns(dbSetMock.Object);
            repositoryMock.Setup(x => x.CreateDbContextAsync(cancellationToken)).ReturnsAsync(dbContextMock.Object);
            configurationMock.Setup(c => c["SlotsPerUser"]).Returns("5");
            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await serverSlotService.CreateServerSlotAsync(serverSlot, cancellationToken));
        }
        [Test]
        public async Task UpdateServerSlotAsync_ValidSlot_SlotUpdated()
        {
            // Arrange
            var serverSlot = new ServerSlot { Id = "1", UserEmail = "test@example.com", Name = "UpdatedSlot", CreationDate = DateTime.UtcNow };
            var serverSlotInDb = new ServerSlot { Id = "1", UserEmail = "test@example.com", Name = "OldSlot", CreationDate = DateTime.UtcNow };
            var dbContextMock = CreateMockDbContext();
            var dbSetMock = GetDbSetMock(new List<ServerSlot> { serverSlotInDb }.AsQueryable());
            dbContextMock.Setup(x => x.ServerSlots).Returns(dbSetMock.Object);
            repositoryMock.Setup(x => x.CreateDbContextAsync(cancellationToken)).ReturnsAsync(dbContextMock.Object);
            // Act
            await serverSlotService.UpdateServerSlotAsync(serverSlot, cancellationToken);
            // Assert
            dbContextMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
            Assert.That(serverSlotInDb.Name, Is.EqualTo(serverSlot.Name));
        }
        [Test]
        public async Task DeleteServerSlotByIdAsync_ValidId_SlotDeleted()
        {
            // Arrange
            var serverSlot = new ServerSlot { Id = "1", UserEmail = "test@example.com", Name = "SlotToDelete", CreationDate = DateTime.UtcNow };
            var dbContextMock = CreateMockDbContext();
            var dbSetMock = GetDbSetMock(new List<ServerSlot> { serverSlot }.AsQueryable());
            dbContextMock.Setup(x => x.ServerSlots).Returns(dbSetMock.Object);
            repositoryMock.Setup(x => x.CreateDbContextAsync(cancellationToken)).ReturnsAsync(dbContextMock.Object);
            // Act
            await serverSlotService.DeleteServerSlotByIdAsync(serverSlot.Id, cancellationToken);
            // Assert
            dbSetMock.Verify(x => x.Remove(serverSlot), Times.Once);
            dbContextMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
        }
    }
}