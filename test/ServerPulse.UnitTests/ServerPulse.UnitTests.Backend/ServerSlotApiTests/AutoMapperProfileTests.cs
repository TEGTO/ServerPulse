using AutoMapper;
using ServerSlotApi;
using ServerSlotApi.Dtos;
using ServerSlotApi.Infrastructure.Entities;

namespace ServerSlotApiTests
{
    [TestFixture]
    public class AutoMapperProfileTests
    {
        private IMapper mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });
            mapper = config.CreateMapper();
        }

        [Test]
        public void Map_ServerSlot_To_ServerSlotResponse()
        {
            // Arrange
            var serverSlot = new ServerSlot
            {
                Id = "1",
                UserEmail = "test@example.com",
                Name = "SlotName",
            };

            // Act
            var result = mapper.Map<ServerSlotResponse>(serverSlot);

            // Assert
            Assert.That(result.Id, Is.EqualTo(serverSlot.Id));
            Assert.That(result.UserEmail, Is.EqualTo(serverSlot.UserEmail));
            Assert.That(result.Name, Is.EqualTo(serverSlot.Name));
            Assert.That(result.SlotKey, Is.EqualTo(serverSlot.SlotKey));
        }

        [Test]
        public void Map_CreateServerSlotRequest_To_ServerSlot()
        {
            // Arrange
            var createRequest = new CreateServerSlotRequest
            {
                Name = "NewSlot"
            };

            // Act
            var result = mapper.Map<ServerSlot>(createRequest);

            // Assert
            Assert.That(result.Name, Is.EqualTo(createRequest.Name));
            Assert.That(result.UserEmail, Is.EqualTo(default));
            Assert.That(result.SlotKey, Is.Not.EqualTo(default));
            Assert.That(result.CreationDate, Is.Not.EqualTo(default(DateTime)));
        }

        [Test]
        public void Map_UpdateServerSlotRequest_To_ServerSlot()
        {
            // Arrange
            var updateRequest = new UpdateServerSlotRequest
            {
                Id = "1",
                Name = "UpdatedSlot"
            };

            // Act
            var result = mapper.Map<ServerSlot>(updateRequest);

            // Assert
            Assert.That(result.Id, Is.EqualTo(updateRequest.Id));
            Assert.That(result.Name, Is.EqualTo(updateRequest.Name));
            Assert.That(result.UserEmail, Is.EqualTo(default));
            Assert.That(result.SlotKey, Is.Not.EqualTo(default));
            Assert.That(result.CreationDate, Is.Not.EqualTo(default(DateTime)));
        }
    }
}