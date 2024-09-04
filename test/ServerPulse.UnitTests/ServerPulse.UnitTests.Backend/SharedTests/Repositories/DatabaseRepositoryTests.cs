using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MockQueryable.Moq;
using Moq;

namespace Shared.Repositories.Tests
{
    [TestFixture]
    internal class DatabaseRepositoryTests
    {
        private Mock<IDbContextFactory<MockDbContext>> dbContextFactoryMock;
        private MockRepository mockRepository;
        private Mock<MockDbContext> mockDbContext;
        private Mock<DatabaseFacade> mockDatabase;
        private CancellationToken cancellationToken;
        private DatabaseRepository<MockDbContext> repository;

        [SetUp]
        public void SetUp()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            mockDbContext = new Mock<MockDbContext>(new DbContextOptionsBuilder<MockDbContext>()
                .UseSqlite("Filename=:memory:")
                .Options);

            mockDatabase = new Mock<DatabaseFacade>(mockDbContext.Object);

            var dbSetMock = GetDbSetMock(new List<TestEntity> {
                 new TestEntity { Id = 1, Name = "Test" },
                  new TestEntity { Id = 2, Name = "Test2" }
            }.AsQueryable());
            mockDbContext.Setup(x => x.Set<TestEntity>()).Returns(dbSetMock.Object);
            mockDbContext.Setup(x => x.Database).Returns(mockDatabase.Object);

            dbContextFactoryMock = new Mock<IDbContextFactory<MockDbContext>>();
            dbContextFactoryMock.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                                .ReturnsAsync(mockDbContext.Object);

            repository = new DatabaseRepository<MockDbContext>(dbContextFactoryMock.Object);
            cancellationToken = new CancellationToken();
        }
        private static Mock<DbSet<T>> GetDbSetMock<T>(IQueryable<T> data) where T : class
        {
            return data.BuildMockDbSet();
        }

        [Test]
        public async Task MigrateDatabaseAsync_ValidCall_DatabaseMigrated()
        {
            // Act
            await repository.MigrateDatabaseAsync(cancellationToken);
            // Assert
            dbContextFactoryMock.Verify(factory => factory.CreateDbContextAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test]
        public async Task AddAsync_ValidObject_ObjectAdded()
        {
            // Arrange
            var testEntity = new TestEntity { Id = 3, Name = "Test3" };
            // Act
            var result = await repository.AddAsync(testEntity, cancellationToken);
            // Assert
            mockDbContext.Verify(x => x.AddAsync(testEntity, It.IsAny<CancellationToken>()), Times.Once);
        }
        [Test]
        public async Task GetQueryableAsync_ValidCall_ReturnsQueryable()
        {
            // Act
            var queryable = await repository.GetQueryableAsync<TestEntity>(cancellationToken);
            // Assert
            Assert.IsInstanceOf<IQueryable<TestEntity>>(queryable);
            Assert.That(queryable.Count(), Is.EqualTo(2));
        }
        [Test]
        public async Task UpdateAsync_ValidObject_ObjectUpdated()
        {
            // Arrange
            var testEntity = new TestEntity { Id = 1, Name = "NewName" };
            // Act
            await repository.UpdateAsync(testEntity, cancellationToken);
            // Assert
            mockDbContext.Verify(x => x.Update(testEntity), Times.Once);
        }
        [Test]
        public async Task DeleteAsync_ValidObject_ObjectDeleted()
        {
            // Arrange
            var testEntity = new TestEntity { Id = 1, Name = "Test" };
            // Act
            await repository.DeleteAsync(testEntity, cancellationToken);
            // Assert
            mockDbContext.Verify(x => x.Remove(testEntity), Times.Once);
        }
    }

    public class MockDbContext : DbContext
    {
        public MockDbContext(DbContextOptions<MockDbContext> options) : base(options) { }
    }

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}