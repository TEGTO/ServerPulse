using Microsoft.Extensions.Logging;
using Shared;

namespace SharedTests
{
    [TestFixture]
    public class ExtensionsTests
    {
        private static readonly ILogger logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("Extensions");

        [Test]
        public void TryToDeserialize_ValidJson_ShouldReturnTrueAndObject()
        {
            // Arrange
            var json = "{\"Name\":\"John\",\"Age\":30}";
            Person person;

            // Act
            var result = json.TryToDeserialize(out person);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(person);
            Assert.That(person.Name, Is.EqualTo("John"));
            Assert.That(person.Age, Is.EqualTo(30));
        }
        [Test]
        public void TryToDeserialize_InvalidJson_ShouldReturnFalseAndDefaultObject()
        {
            // Arrange
            var invalidJson = "{\"Name\":\"John\",\"Age\":}";
            Person person;
            // Act
            var result = invalidJson.TryToDeserialize(out person);
            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(person);
        }
        [Test]
        public void TryToDeserialize_NullOrEmptyString_ShouldReturnFalseAndDefaultObject()
        {
            // Arrange
            string nullJson = null;
            Person person;
            // Act
            var resultNull = nullJson.TryToDeserialize(out person);
            // Assert
            Assert.IsFalse(resultNull);
            Assert.IsNull(person);
            // Arrange
            string emptyJson = string.Empty;
            // Act
            var resultEmpty = emptyJson.TryToDeserialize(out person);
            // Assert
            Assert.IsFalse(resultEmpty);
            Assert.IsNull(person);
        }
        [Test]
        public void TryToDeserialize_JsonWithNullObject_ShouldReturnFalse()
        {
            // Arrange
            var jsonWithNullObject = "null";
            Person person;
            // Act
            var result = jsonWithNullObject.TryToDeserialize(out person);
            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(person);
        }
        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}