using DatabaseControl.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MockQueryable.Moq;
using Moq;
using Polly;
using Polly.Registry;

namespace DatabaseControl.Tests
{
    [TestFixture]
    internal class DatabaseRepositoryTests
    {
        private Mock<IDbContextFactory<MockDbContext>> dbContextFactoryMock;
        private Mock<MockDbContext> mockDbContext;
        private Mock<DatabaseFacade> mockDatabase;
        private DatabaseRepository<MockDbContext> repository;
        private CancellationToken cancellationToken;

        [SetUp]
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
            mockDbContext.Setup(x => x.Database).Returns(mockDatabase.Object);

            dbContextFactoryMock = new Mock<IDbContextFactory<MockDbContext>>();
            dbContextFactoryMock.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDbContext.Object);

            var mockPipelineProvider = new Mock<ResiliencePipelineProvider<string>>();
            mockPipelineProvider.Setup(x => x.GetPipeline(It.IsAny<string>()))
                .Returns(ResiliencePipeline.Empty);

            repository = new DatabaseRepository<MockDbContext>(dbContextFactoryMock.Object, mockPipelineProvider.Object);
            cancellationToken = new CancellationToken();
        }

        [Test]
        [TestCase(3, "Test3")]
        [TestCase(4, "Test4")]
        public async Task AddAsync_ValidObject_ObjectAdded(int id, string name)
        {
            // Arrange
            var testEntity = new TestEntity { Id = id, Name = name };

            // Act
            await repository.AddAsync(testEntity, cancellationToken);

            // Assert
            mockDbContext.Verify(x => x.AddAsync(testEntity, It.IsAny<CancellationToken>()), Times.Once);
            mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [TestCase(1, "NewName1")]
        [TestCase(2, "NewName2")]
        public async Task UpdateAsync_ValidObject_ObjectUpdated(int id, string name)
        {
            // Arrange
            var testEntity = new TestEntity { Id = id, Name = name };

            // Act
            await repository.UpdateAsync(testEntity, cancellationToken);

            // Assert
            mockDbContext.Verify(x => x.Update(testEntity), Times.Once);
            mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        public async Task DeleteAsync_ValidObject_ObjectDeleted(int id)
        {
            // Arrange
            var testEntity = new TestEntity { Id = id, Name = $"Test{id}" };

            // Act
            await repository.DeleteAsync(testEntity, cancellationToken);

            // Assert
            mockDbContext.Verify(x => x.Remove(testEntity), Times.Once);
            mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [TestCase(3, "BatchName1", 4, "BatchName2")]
        [TestCase(5, "BatchName3", 6, "BatchName4")]
        public async Task UpdateRangeAsync_ValidObjects_ObjectsUpdated(int id1, string name1, int id2, string name2)
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { Id = id1, Name = name1 },
                new TestEntity { Id = id2, Name = name2 }
            };

            // Act
            await repository.UpdateRangeAsync(entities, cancellationToken);

            // Assert
            mockDbContext.Verify(x => x.UpdateRange(entities), Times.Once);
            mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetQueryableAsync_ValidCall_ReturnsQueryable()
        {
            // Act
            var queryable = await repository.GetQueryableAsync<TestEntity>(cancellationToken);

            // Assert
            Assert.IsInstanceOf<IQueryable<TestEntity>>(queryable);
            Assert.That(await queryable.CountAsync(), Is.EqualTo(2));
        }

        [Test]
        public async Task MigrateDatabaseAsync_ValidCall_DatabaseMigrated()
        {
            // Act
            await repository.MigrateDatabaseAsync(cancellationToken);

            // Assert
            dbContextFactoryMock.Verify(factory => factory.CreateDbContextAsync(It.IsAny<CancellationToken>()), Times.Once);
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