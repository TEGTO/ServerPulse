﻿using DatabaseControl.Repositories;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using ServerSlotApi.Infrastructure.Data;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Models;

namespace ServerSlotApi.Infrastructure.Repositories.Tests
{
    [TestFixture]
    internal class ServerSlotRepositoryTests
    {
        private Mock<IDatabaseRepository<ServerSlotDbContext>> repositoryMock;
        private ServerSlotRepository slotRepository;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IDatabaseRepository<ServerSlotDbContext>>();

            slotRepository = new ServerSlotRepository(repositoryMock.Object);
            cancellationToken = new CancellationToken();
        }

        private static Mock<DbSet<T>> GetDbSetMock<T>(List<T> data) where T : class
        {
            return data.AsQueryable().BuildMockDbSet();
        }

        private static IEnumerable<TestCaseData> GetSlotTestCases()
        {
            yield return new TestCaseData(new SlotModel { UserEmail = "user1@example.com", SlotId = "1" }, "Slot1", true)
                .SetDescription("Slot exists with matching UserEmail and SlotId.");

            yield return new TestCaseData(new SlotModel { UserEmail = "user1@example.com", SlotId = "99" }, null, false)
                .SetDescription("Slot does not exist with this id.");

            yield return new TestCaseData(new SlotModel { UserEmail = "user2@example.com", SlotId = "1" }, null, false)
                .SetDescription("Slot does not exist with this user email.");

            yield return new TestCaseData(new SlotModel { UserEmail = "user2@example.com", SlotId = "99" }, null, false)
                .SetDescription("Slot does not exist.");
        }

        [Test]
        [TestCaseSource(nameof(GetSlotTestCases))]
        public async Task GetSlotAsync_TestCases(SlotModel model, string? expectedSlotName, bool shouldExist)
        {
            // Arrange
            var slots = new List<ServerSlot>
            {
                new ServerSlot { Id = "1", UserEmail = "user1@example.com", Name = "Slot1" },
                new ServerSlot { Id = "2", UserEmail = "user1@example.com", Name = "Slot2" }
            };
            var dbSetMock = GetDbSetMock(slots);

            repositoryMock.Setup(repo => repo.GetQueryableAsync<ServerSlot>(cancellationToken))
                .ReturnsAsync(dbSetMock.Object);

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

            repositoryMock.Verify(repo => repo.GetQueryableAsync<ServerSlot>(cancellationToken), Times.Once);
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

            repositoryMock.Setup(repo => repo.GetQueryableAsync<ServerSlot>(cancellationToken))
                .ReturnsAsync(dbSetMock.Object);

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

            repositoryMock.Verify(repo => repo.GetQueryableAsync<ServerSlot>(cancellationToken), Times.Once);
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

            repositoryMock.Setup(repo => repo.GetQueryableAsync<ServerSlot>(cancellationToken))
                .ReturnsAsync(dbSetMock.Object);

            // Act
            var result = await slotRepository.GetSlotsByUserEmailAsync(email, nameFilter, cancellationToken);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(expectedCount));

            repositoryMock.Verify(repo => repo.GetQueryableAsync<ServerSlot>(cancellationToken), Times.Once);
        }

        private static IEnumerable<TestCaseData> CreateSlotTestCases()
        {
            yield return new TestCaseData("user1@example.com", "New Slot")
                .SetDescription("Creates a valid slot.");
        }

        [Test]
        [TestCaseSource(nameof(CreateSlotTestCases))]
        public async Task CreateSlotAsync_TestCases(string userEmail, string slotName)
        {
            // Arrange
            var slot = new ServerSlot { UserEmail = userEmail, Name = slotName };

            repositoryMock.Setup(repo => repo.AddAsync(slot, cancellationToken))
                .ReturnsAsync(slot);

            // Act
            var result = await slotRepository.CreateSlotAsync(slot, cancellationToken);

            // Assert
            Assert.That(result.UserEmail, Is.EqualTo(userEmail));
            Assert.That(result.Name, Is.EqualTo(slotName));
            Assert.IsNotEmpty(result.SlotKey);

            repositoryMock.Verify(repo => repo.AddAsync(slot, cancellationToken), Times.Once);
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
            repositoryMock.Verify(repo => repo.UpdateAsync(slot, cancellationToken), Times.Once);
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
            repositoryMock.Verify(repo => repo.DeleteAsync(slot, cancellationToken), Times.Once);
        }
    }
}