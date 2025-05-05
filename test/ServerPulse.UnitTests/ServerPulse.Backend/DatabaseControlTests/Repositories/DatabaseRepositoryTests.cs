using DatabaseControl.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using MockQueryable.Moq;
using Moq;
using System.Data;

namespace DatabaseControl.Tests
{
    [TestFixture]
    internal class DatabaseRepositoryTests
    {
        private Mock<IDbContextFactory<MockDbContext>> mockDbContextFactory;
        private Mock<MockDbContext> mockDbContext;
        private Mock<DatabaseFacade> mockDatabase;
        private DatabaseRepository<MockDbContext> repository;
        private CancellationToken cancellationToken;

        [SetUp]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<Pending>")]
        public void SetUp()
        {
            mockDbContext = new Mock<MockDbContext>(
                new DbContextOptionsBuilder<MockDbContext>().UseSqlite("Filename=:memory:").Options
            );

            mockDatabase = new Mock<DatabaseFacade>(mockDbContext.Object);

            var dbSetMock = new List<TestEntity>
            {
                new TestEntity { Id = 1, Name = "Test" },
                new TestEntity { Id = 2, Name = "Test2" }
            }.AsQueryable().BuildMockDbSet();

            mockDbContext.Setup(x => x.Set<TestEntity>()).Returns(dbSetMock.Object);
            mockDbContext.Setup(x => x.Set<TestEntity>(It.IsAny<string>())).Returns(dbSetMock.Object);
            mockDbContext.Setup(x => x.Database).Returns(mockDatabase.Object);
            mockDbContext.Setup(x => x.Update(It.IsAny<TestEntity>()))
                .Returns((TestEntity entity) =>
                {
                    var internalEntityEntry = new InternalEntityEntry(
                       new Mock<IStateManager>().Object,
                       new RuntimeEntityType("TestEntity", typeof(TestEntity), false, null!, null, null, ChangeTrackingStrategy.Snapshot, null, false, entity),
                       entity
                    );

                    var entityEntry = new Mock<EntityEntry<TestEntity>>(internalEntityEntry);

                    return entityEntry.Object;
                });
            mockDbContext.Setup(x => x.AddAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((TestEntity entity, CancellationToken ct) =>
              {
                  var internalEntityEntry = new InternalEntityEntry(
                     new Mock<IStateManager>().Object,
                     new RuntimeEntityType("TestEntity", typeof(TestEntity), false, null!, null, null, ChangeTrackingStrategy.Snapshot, null, false, entity),
                     entity
                  );

                  var entityEntry = new Mock<EntityEntry<TestEntity>>(internalEntityEntry);

                  return entityEntry.Object;
              });

            mockDbContextFactory = new Mock<IDbContextFactory<MockDbContext>>();
            mockDbContextFactory.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDbContext.Object);

            repository = new DatabaseRepository<MockDbContext>(mockDbContextFactory.Object);
            cancellationToken = new CancellationToken();
        }

        [Test]
        public async Task MigrateDatabaseAsync_ValidCall_DatabaseMigrated()
        {
            // Act
            await repository.MigrateDatabaseAsync(cancellationToken);

            // Assert
            mockDbContextFactory.Verify(factory => factory.CreateDbContextAsync(cancellationToken), Times.Once);
            mockDbContext.Verify(x => x.Database, Times.Once);
        }

        [Test]
        public async Task GetDbContextAsync_ValidCall_ReturnsNewDbContext()
        {
            // Act
            var dbContext = await repository.GetDbContextAsync(cancellationToken);

            // Assert
            Assert.IsInstanceOf<MockDbContext>(dbContext);
            mockDbContextFactory.Verify(x => x.CreateDbContextAsync(cancellationToken), Times.Once);
        }

        [Test]
        public async Task Query_ValidCall_ReturnsQueryable()
        {
            // Act
            var dbContext = mockDbContext.Object;
            var queryable = repository.Query<TestEntity>(dbContext);

            // Assert
            Assert.IsInstanceOf<IQueryable<TestEntity>>(queryable);
            Assert.That(await queryable.CountAsync(), Is.EqualTo(2));
        }

        [Test]
        public async Task QueryWithSetName_ValidCall_ReturnsQueryable()
        {
            // Act
            var dbContext = mockDbContext.Object;
            var queryable = repository.Query<TestEntity>(dbContext, "TestEntities");

            // Assert
            Assert.IsInstanceOf<IQueryable<TestEntity>>(queryable);
            Assert.That(await queryable.CountAsync(), Is.EqualTo(2));
        }

        [Test]
        [TestCase(3, "Test3")]
        [TestCase(4, "Test4")]
        public async Task AddAsync_ValidObject_ObjectAdded(int id, string name)
        {
            // Arrange
            var dbContext = mockDbContext.Object;
            var testEntity = new TestEntity { Id = id, Name = name };

            // Act
            await repository.AddAsync(dbContext, testEntity, cancellationToken);

            // Assert
            mockDbContext.Verify(x => x.AddAsync(testEntity, cancellationToken), Times.Once);
        }

        [Test]
        [TestCase(1, "NewName1")]
        [TestCase(2, "NewName2")]
        public void Update_ValidObject_ObjectUpdated(int id, string name)
        {
            // Arrange
            var dbContext = mockDbContext.Object;
            var testEntity = new TestEntity { Id = id, Name = name };

            // Act
            repository.Update(dbContext, testEntity);

            // Assert
            mockDbContext.Verify(x => x.Update(testEntity), Times.Once);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        public void Remove_ValidObject_ObjectDeleted(int id)
        {
            // Arrange
            var dbContext = mockDbContext.Object;
            var testEntity = new TestEntity { Id = id, Name = $"Test{id}" };

            // Act
            repository.Remove(dbContext, testEntity);

            // Assert
            mockDbContext.Verify(x => x.Remove(testEntity), Times.Once);
        }

        [Test]
        public async Task SaveChangesAsync_SavesChangesValid()
        {
            // Arrange
            var dbContext = mockDbContext.Object;

            // Act
            await repository.SaveChangesAsync(dbContext, cancellationToken);

            // Assert
            mockDbContext.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
        }

        [Test]
        public async Task BeginTransactionAsync_StartsTransactionValid()
        {
            // Arrange
            var dbContext = mockDbContext.Object;

            // Act
            await repository.BeginTransactionAsync(dbContext, IsolationLevel.Unspecified, cancellationToken);

            // Assert
            mockDbContext.Verify(x => x.Database, Times.Once);
        }
    }

    public class MockDbContext : DbContext
    {
        public MockDbContext(DbContextOptions<MockDbContext> options) : base(options) { }
    }

    public class TestEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}