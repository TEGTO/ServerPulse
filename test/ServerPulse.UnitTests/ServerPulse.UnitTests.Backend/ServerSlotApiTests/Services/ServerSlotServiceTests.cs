using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using ServerSlotApi;
using ServerSlotApi.Data;
using ServerSlotApi.Domain.Entities;
using ServerSlotApi.Services;
using Shared.Repositories;

namespace ServerSlotApiTests.Services
{
    [TestFixture]
    internal class ServerSlotServiceTests
    {
        private const int SERVER_SLOTS_PER_USER = 3;

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
            configurationMock.Setup(config => config[Configuration.SERVER_SLOTS_PER_USER])
                .Returns(SERVER_SLOTS_PER_USER.ToString());
            repositoryMock = new Mock<IDatabaseRepository<ServerDataDbContext>>();
            serverSlotService = new ServerSlotService(repositoryMock.Object, configurationMock.Object);
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
        public async Task GetSlotByIdAsync_ValidParams_ReturnsSlot()
        {
            // Arrange
            var slotParams = new SlotParams("test@example.com", "1");
            var serverSlot = new ServerSlot { Id = "1", UserEmail = "test@example.com", Name = "Slot1" };
            var serverSlots = new List<ServerSlot> { serverSlot }.AsQueryable();

            var dbSetMock = GetDbSetMock(serverSlots);
            repositoryMock.Setup(x => x.GetQueryableAsync<ServerSlot>(cancellationToken)).ReturnsAsync(dbSetMock.Object);

            // Act
            var result = await serverSlotService.GetSlotByIdAsync(slotParams, cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(serverSlot));
        }

        [Test]
        public async Task GetSlotByIdAsync_InvalidParams_ReturnsNull()
        {
            // Arrange
            var slotParams = new SlotParams("test@example.com", "1");
            var serverSlots = new List<ServerSlot>().AsQueryable();

            var dbSetMock = GetDbSetMock(serverSlots);
            repositoryMock.Setup(x => x.GetQueryableAsync<ServerSlot>(cancellationToken)).ReturnsAsync(dbSetMock.Object);

            // Act
            var result = await serverSlotService.GetSlotByIdAsync(slotParams, cancellationToken);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetSlotsByEmailAsync_ValidEmail_ReturnsServerSlots()
        {
            // Arrange
            var email = "test@example.com";
            var serverSlots = new List<ServerSlot>
            {
                new ServerSlot { UserEmail = email, Name = "Slot1", CreationDate = DateTime.UtcNow },
                new ServerSlot { UserEmail = email, Name = "Slot2", CreationDate = DateTime.UtcNow }
            }.AsQueryable();
            var dbSetMock = GetDbSetMock(serverSlots);
            repositoryMock.Setup(x => x.GetQueryableAsync<ServerSlot>(cancellationToken)).ReturnsAsync(dbSetMock.Object);

            // Act
            var result = await serverSlotService.GetSlotsByEmailAsync(email, cancellationToken);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Name, Is.EqualTo("Slot2"));
        }

        [Test]
        public async Task GetSlotsContainingStringAsync_ValidEmailAndString_ReturnsServerSlots()
        {
            // Arrange
            var email = "test@example.com";
            var searchString = "Slot";
            var serverSlots = new List<ServerSlot>
            {
                new ServerSlot { UserEmail = email, Name = "Slot1", CreationDate = DateTime.UtcNow },
                new ServerSlot { UserEmail = email, Name = "Slot2", CreationDate = DateTime.UtcNow },
                new ServerSlot { UserEmail = email, Name = "Another", CreationDate = DateTime.UtcNow }
            }.AsQueryable();

            var dbSetMock = GetDbSetMock(serverSlots);
            repositoryMock.Setup(x => x.GetQueryableAsync<ServerSlot>(cancellationToken)).ReturnsAsync(dbSetMock.Object);
            // Act
            var result = await serverSlotService.GerSlotsContainingStringAsync(email, searchString, cancellationToken);
            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.IsTrue(result.All(x => x.Name.Contains(searchString)));
        }

        [Test]
        public async Task CheckIfKeyValidAsync_ValidKey_ReturnsTrue()
        {
            // Arrange
            var key = "valid-key";
            var serverSlots = new List<ServerSlot>
            {
                new ServerSlot { SlotKey = key }
            }.AsQueryable();

            var dbSetMock = GetDbSetMock(serverSlots);
            repositoryMock.Setup(x => x.GetQueryableAsync<ServerSlot>(cancellationToken)).ReturnsAsync(dbSetMock.Object);

            // Act
            var result = await serverSlotService.CheckIfKeyValidAsync(key, cancellationToken);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task CheckIfKeyValidAsync_InvalidKey_ReturnsFalse()
        {
            // Arrange
            var key = "invalid-key";
            var serverSlots = new List<ServerSlot>
            {
                new ServerSlot { SlotKey = "some-other-key" }
            }.AsQueryable();

            var dbSetMock = GetDbSetMock(serverSlots);
            repositoryMock.Setup(x => x.GetQueryableAsync<ServerSlot>(cancellationToken)).ReturnsAsync(dbSetMock.Object);

            // Act
            var result = await serverSlotService.CheckIfKeyValidAsync(key, cancellationToken);

            // Assert
            Assert.IsFalse(result);
        }
        [Test]
        public async Task CreateSlotAsync_ValidSlot_SlotCreated()
        {
            // Arrange
            var serverSlot = new ServerSlot { UserEmail = "test@example.com", Name = "NewSlot", CreationDate = DateTime.UtcNow };
            repositoryMock.Setup(repo => repo.AddAsync(serverSlot, cancellationToken)).ReturnsAsync(serverSlot);

            // Act
            var result = await serverSlotService.CreateSlotAsync(serverSlot, cancellationToken);

            // Assert
            repositoryMock.Verify(repo => repo.AddAsync(serverSlot, cancellationToken), Times.Once);
            Assert.That(result, Is.EqualTo(serverSlot));
        }

        [Test]
        public async Task UpdateSlotAsync_ValidSlot_SlotUpdated()
        {
            // Arrange
            var slotParams = new SlotParams("test@example.com", "1");
            var serverSlot = new ServerSlot { Id = "1", UserEmail = "test@example.com", Name = "UpdatedSlot", CreationDate = DateTime.UtcNow };
            var serverSlotInDb = new ServerSlot { Id = "1", UserEmail = "test@example.com", Name = "OldSlot", CreationDate = DateTime.UtcNow };

            var dbContextMock = CreateMockDbContext();
            var dbSetMock = GetDbSetMock(new List<ServerSlot> { serverSlotInDb }.AsQueryable());
            dbContextMock.Setup(x => x.ServerSlots).Returns(dbSetMock.Object);
            repositoryMock.Setup(repo => repo.GetQueryableAsync<ServerSlot>(cancellationToken)).ReturnsAsync(dbSetMock.Object.AsQueryable());

            // Act
            await serverSlotService.UpdateSlotAsync(slotParams, serverSlot, cancellationToken);

            // Assert
            repositoryMock.Verify(repo => repo.UpdateAsync(serverSlotInDb, cancellationToken), Times.Once);
            Assert.That(serverSlotInDb.Name, Is.EqualTo(serverSlot.Name));
        }

        [Test]
        public async Task DeleteSlotByIdAsync_ValidId_SlotDeleted()
        {
            // Arrange
            var slotParams = new SlotParams("test@example.com", "1");
            var serverSlot = new ServerSlot { Id = "1", UserEmail = "test@example.com", Name = "SlotToDelete", CreationDate = DateTime.UtcNow };

            var dbContextMock = CreateMockDbContext();
            var dbSetMock = GetDbSetMock(new List<ServerSlot> { serverSlot }.AsQueryable());
            dbContextMock.Setup(x => x.ServerSlots).Returns(dbSetMock.Object);
            repositoryMock.Setup(repo => repo.GetQueryableAsync<ServerSlot>(cancellationToken)).ReturnsAsync(dbSetMock.Object.AsQueryable());

            // Act
            await serverSlotService.DeleteSlotByIdAsync(slotParams, cancellationToken);

            // Assert
            repositoryMock.Verify(repo => repo.DeleteAsync(serverSlot, cancellationToken), Times.Once);
        }
    }
}