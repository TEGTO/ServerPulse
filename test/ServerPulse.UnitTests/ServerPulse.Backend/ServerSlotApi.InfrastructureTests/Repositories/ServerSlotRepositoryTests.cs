using DatabaseControl.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using ServerSlotApi.Core.Entities;
using ServerSlotApi.Core.Models;
using ServerSlotApi.Infrastructure.Data;
using System.Data;

namespace ServerSlotApi.Infrastructure.Repositories.Tests
{
    [TestFixture]
    internal class ServerSlotRepositoryTests
    {
        private Mock<IDatabaseRepository<ServerSlotDbContext>> repositoryMock;
        private Mock<IConfiguration> configurationMock;
        private Mock<IDbContextTransaction> transactionMock;
        private ServerSlotRepository slotRepository;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IDatabaseRepository<ServerSlotDbContext>>();
            configurationMock = new Mock<IConfiguration>();
            transactionMock = new Mock<IDbContextTransaction>();

            repositoryMock.Setup(x => x.GetDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => null!);
            repositoryMock.Setup(x => x.BeginTransactionAsync(It.IsAny<ServerSlotDbContext>(), It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionMock.Object);
            configurationMock.Setup(x => x[ConfigurationKeys.SERVER_SLOTS_PER_USER]).Returns("5");

            slotRepository = new ServerSlotRepository(repositoryMock.Object, configurationMock.Object);
            cancellationToken = new CancellationToken();
        }

        private static Mock<DbSet<T>> GetDbSetMock<T>(List<T> data) where T : class
        {
            return data.AsQueryable().BuildMockDbSet();
        }

        private static IEnumerable<TestCaseData> GetSlotTestCases()
        {
            yield return new TestCaseData(new GetSlot { UserEmail = "user1@example.com", SlotId = "1" }, "Slot1", true)
                .SetDescription("Slot exists with matching UserEmail and SlotId.");

            yield return new TestCaseData(new GetSlot { UserEmail = "user1@example.com", SlotId = "99" }, null, false)
                .SetDescription("Slot does not exist with this id.");

            yield return new TestCaseData(new GetSlot { UserEmail = "user2@example.com", SlotId = "1" }, null, false)
                .SetDescription("Slot does not exist with this user email.");

            yield return new TestCaseData(new GetSlot { UserEmail = "user2@example.com", SlotId = "99" }, null, false)
                .SetDescription("Slot does not exist.");
        }

        [Test]
        [TestCaseSource(nameof(GetSlotTestCases))]
        public async Task GetSlotAsync_TestCases(GetSlot model, string? expectedSlotName, bool shouldExist)
        {
            // Arrange
            var slots = new List<ServerSlot>
            {
                new ServerSlot { Id = "1", UserEmail = "user1@example.com", Name = "Slot1" },
                new ServerSlot { Id = "2", UserEmail = "user1@example.com", Name = "Slot2" }
            };
            var dbSetMock = GetDbSetMock(slots);

            repositoryMock.Setup(repo => repo.Query<ServerSlot>(It.IsAny<ServerSlotDbContext>()))
                .Returns(dbSetMock.Object);

            // Act
            var result = await slotRepository.GetSlotAsync(model, cancellationToken);

            // Assert
            if (shouldExist)
            {
                Assert.IsNotNull(result);
                Assert.That(result.Name, Is.EqualTo(expectedSlotName));
                Assert.That(result.UserEmail, Is.EqualTo(model.UserEmail));
                Assert.That(result.Id, Is.EqualTo(model.SlotId));
            }
            else
            {
                Assert.IsNull(result);
            }

            repositoryMock.Verify(repo => repo.GetDbContextAsync(cancellationToken), Times.Once);
            repositoryMock.Verify(repo => repo.Query<ServerSlot>(It.IsAny<ServerSlotDbContext>()), Times.Once);
        }

        private static IEnumerable<TestCaseData> GetSlotByKeyTestCases()
        {
            var slots = new List<ServerSlot>
            {
                new ServerSlot { Id = "1", UserEmail = "user1@example.com", Name = "Slot1", SlotKey = "some valid key" },
                new ServerSlot { Id = "2", UserEmail = "user1@example.com", Name = "Slot2" }
            };

            yield return new TestCaseData(slots, "some valid key", "Slot1", true)
                .SetDescription("Slot exists with matching key.");

            yield return new TestCaseData(slots, "some invalid key", null, false)
                .SetDescription("Slot does not exist with this key.");
        }

        [Test]
        [TestCaseSource(nameof(GetSlotByKeyTestCases))]
        public async Task GetSlotByKeyAsync_TestCases(List<ServerSlot> slots, string key, string? expectedSlotName, bool shouldExist)
        {
            // Arrange
            var dbSetMock = GetDbSetMock(slots);

            repositoryMock.Setup(repo => repo.Query<ServerSlot>(It.IsAny<ServerSlotDbContext>()))
                .Returns(dbSetMock.Object);

            // Act
            var result = await slotRepository.GetSlotByKeyAsync(key, cancellationToken);

            // Assert
            if (shouldExist)
            {
                Assert.IsNotNull(result);
                Assert.That(result.Name, Is.EqualTo(expectedSlotName));
                Assert.That(result.SlotKey, Is.EqualTo(key));
            }
            else
            {
                Assert.IsNull(result);
            }

            repositoryMock.Verify(repo => repo.GetDbContextAsync(cancellationToken), Times.Once);
            repositoryMock.Verify(repo => repo.Query<ServerSlot>(It.IsAny<ServerSlotDbContext>()), Times.Once);
        }

        private static IEnumerable<TestCaseData> GetSlotsByUserEmailTestCases()
        {
            yield return new TestCaseData("user1@example.com", "Slot", 2)
                .SetDescription("Returns all matching slots with UserEmail and partial name.");

            yield return new TestCaseData("user1@example.com", "Invalid", 0)
                .SetDescription("No slots match the name filter.");


            yield return new TestCaseData("user2@example.com", "", 0)
                .SetDescription("No slots match the UserEmail.");
        }

        [Test]
        [TestCaseSource(nameof(GetSlotsByUserEmailTestCases))]
        public async Task GetSlotsByUserEmailAsync_TestCases(string email, string nameFilter, int expectedCount)
        {
            // Arrange
            var slots = new List<ServerSlot>
            {
                new ServerSlot { Id = "1", UserEmail = "user1@example.com", Name = "Slot1" },
                new ServerSlot { Id = "2", UserEmail = "user1@example.com", Name = "Slot2" }
            };
            var dbSetMock = GetDbSetMock(slots);

            repositoryMock.Setup(repo => repo.Query<ServerSlot>(It.IsAny<ServerSlotDbContext>()))
                .Returns(dbSetMock.Object);

            // Act
            var result = await slotRepository.GetSlotsByUserEmailAsync(email, nameFilter, cancellationToken);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(expectedCount));

            repositoryMock.Verify(repo => repo.GetDbContextAsync(cancellationToken), Times.Once);
            repositoryMock.Verify(repo => repo.Query<ServerSlot>(It.IsAny<ServerSlotDbContext>()), Times.Once);
        }

        private static IEnumerable<TestCaseData> CreateSlotTestCases()
        {
            yield return new TestCaseData("user1@example.com", "New Slot")
                .SetDescription("Creates a valid slot for a user.");
        }

        [Test]
        [TestCaseSource(nameof(CreateSlotTestCases))]
        public async Task CreateSlotAsync_ValidSlot_CreatesSlot(string userEmail, string slotName)
        {
            // Arrange
            var slot = new ServerSlot { UserEmail = userEmail, Name = slotName };
            var dbSetMock = GetDbSetMock(new List<ServerSlot>());

            repositoryMock.Setup(repo => repo.Query<ServerSlot>(It.IsAny<ServerSlotDbContext>()))
                .Returns(dbSetMock.Object);

            repositoryMock.Setup(repo => repo.AddAsync(It.IsAny<ServerSlotDbContext>(), slot, cancellationToken))
                .ReturnsAsync(slot);

            // Act
            var result = await slotRepository.CreateSlotAsync(slot, cancellationToken);

            // Assert
            Assert.That(result.UserEmail, Is.EqualTo(userEmail));
            Assert.That(result.Name, Is.EqualTo(slotName));

            repositoryMock.Verify(repo => repo.GetDbContextAsync(cancellationToken), Times.Once);
            repositoryMock.Verify(repo => repo.BeginTransactionAsync(It.IsAny<ServerSlotDbContext>(), IsolationLevel.Serializable, cancellationToken), Times.Once);
            repositoryMock.Verify(repo => repo.Query<ServerSlot>(It.IsAny<ServerSlotDbContext>()), Times.Once);
            repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<ServerSlotDbContext>(), slot, cancellationToken), Times.Once);
            repositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<ServerSlotDbContext>(), cancellationToken), Times.Once);
            transactionMock.Verify(repo => repo.CommitAsync(cancellationToken), Times.Once);
        }

        [Test]
        public void CreateSlotAsync_ExceedsMaxSlots_ThrowsException()
        {
            // Arrange
            var userEmail = "user1@example.com";
            var slot = new ServerSlot { UserEmail = userEmail, Name = "New Slot" };
            var existingSlots = Enumerable.Range(1, 5).Select(i => new ServerSlot { UserEmail = userEmail }).ToList();
            var dbSetMock = GetDbSetMock(existingSlots);

            repositoryMock.Setup(repo => repo.Query<ServerSlot>(It.IsAny<ServerSlotDbContext>()))
                .Returns(dbSetMock.Object);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => slotRepository.CreateSlotAsync(slot, cancellationToken));

            repositoryMock.Verify(repo => repo.GetDbContextAsync(cancellationToken), Times.Once);
            repositoryMock.Verify(repo => repo.BeginTransactionAsync(It.IsAny<ServerSlotDbContext>(), IsolationLevel.Serializable, cancellationToken), Times.Once);
            repositoryMock.Verify(repo => repo.Query<ServerSlot>(It.IsAny<ServerSlotDbContext>()), Times.Once);
            transactionMock.Verify(repo => repo.RollbackAsync(cancellationToken), Times.Once);

            repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<ServerSlotDbContext>(), slot, cancellationToken), Times.Never);
            repositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<ServerSlotDbContext>(), cancellationToken), Times.Never);
            transactionMock.Verify(repo => repo.CommitAsync(cancellationToken), Times.Never);
        }

        private static IEnumerable<TestCaseData> UpdateSlotTestCases()
        {
            yield return new TestCaseData("Updated Slot")
                .SetDescription("Updates an existing slot.");
        }

        [Test]
        [TestCaseSource(nameof(UpdateSlotTestCases))]
        public async Task UpdateSlotAsync_TestCases(string updatedName)
        {
            // Arrange
            var slot = new ServerSlot { Id = "1", UserEmail = "user1@example.com", Name = updatedName };

            // Act
            await slotRepository.UpdateSlotAsync(slot, cancellationToken);

            // Assert
            repositoryMock.Verify(repo => repo.GetDbContextAsync(cancellationToken), Times.Once);
            repositoryMock.Verify(repo => repo.Update(It.IsAny<ServerSlotDbContext>(), slot), Times.Once);
            repositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<ServerSlotDbContext>(), cancellationToken), Times.Once);
        }

        private static IEnumerable<TestCaseData> DeleteSlotTestCases()
        {
            yield return new TestCaseData("SlotToDelete", "user1@example.com")
                .SetDescription("Deletes an existing slot.");
        }

        [Test]
        [TestCaseSource(nameof(DeleteSlotTestCases))]
        public async Task DeleteSlotAsync_TestCases(string slotName, string userEmail)
        {
            // Arrange
            var slot = new ServerSlot { Id = "1", UserEmail = userEmail, Name = slotName };

            // Act
            await slotRepository.DeleteSlotAsync(slot, cancellationToken);

            // Assert
            repositoryMock.Verify(repo => repo.GetDbContextAsync(cancellationToken), Times.Once);
            repositoryMock.Verify(repo => repo.Remove(It.IsAny<ServerSlotDbContext>(), slot), Times.Once);
            repositoryMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<ServerSlotDbContext>(), cancellationToken), Times.Once);
        }
    }
}