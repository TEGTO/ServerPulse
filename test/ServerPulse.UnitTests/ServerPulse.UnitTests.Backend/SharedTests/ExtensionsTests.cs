using Shared;

namespace SharedTests
{
    [TestFixture]
    internal class ExtensionsTests
    {
        [TestCase("{\"Id\":1,\"Name\":\"Test\"}", true, 1, "Test", Description = "Valid JSON")]
        [TestCase("{}", true, 0, null, Description = "Valid but empty object")]
        [TestCase("", false, 0, null, Description = "Empty string")]
        [TestCase(null, false, 0, null, Description = "Null string")]
        [TestCase("{\"Id\":1,\"Name\":\"Test\"", false, 0, null, Description = "Invalid JSON (missing closing brace)")]
        [TestCase("Invalid JSON", false, 0, null, Description = "Completely invalid JSON")]
        public void TryToDeserialize_WithVariousInputs_ReturnsExpectedResults(
         string? input,
         bool expectedResult,
         int expectedId,
         string? expectedName)
        {
            // Act
            var result = input.TryToDeserialize<TestObject>(out var obj);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));

            if (expectedResult)
            {
                Assert.IsNotNull(obj);
                Assert.That(obj?.Id, Is.EqualTo(expectedId));
                Assert.That(obj?.Name, Is.EqualTo(expectedName));
            }
            else
            {
                Assert.IsNull(obj);
            }
        }
    }

    internal class TestObject
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}