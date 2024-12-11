namespace Shared.Helpers.Tests
{
    [TestFixture]
    internal class HashHelperTests
    {
        [Test]
        [TestCase("test", "n4bQgYhMfWWaL-qgxVrQFaO_TxsrC4Is0V1sFbDwCgg")]
        [TestCase("hello", "LPJNul-wow4m6DsqxbninhsWHlwfp0JecwQzYpOLmCQ")]
        [TestCase("123456", "jZae727K08KaOmKSgOaGzww_XVqGr_PKEgIMkjrcbJI")]
        [TestCase("", "47DEQpj8HBSa-_TImW-5JCeuQeRkm5NMpJWZG3hSuFU")]
        public void ComputeHash_ValidInput_ReturnsExpectedHash(string input, string expectedHash)
        {
            // Act
            var actualHash = HashHelper.ComputeHash(input);

            // Assert
            Assert.That(actualHash, Is.EqualTo(expectedHash));
        }

        [Test]
        public void ComputeHash_NullInput_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => HashHelper.ComputeHash(null!));
        }
    }
}