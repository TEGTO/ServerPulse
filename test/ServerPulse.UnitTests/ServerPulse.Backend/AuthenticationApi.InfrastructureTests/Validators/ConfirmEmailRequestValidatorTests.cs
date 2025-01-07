using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.ConfirmEmail;
using FluentValidation.TestHelper;

namespace AuthenticationApi.Infrastructure.Validators.Tests
{
    [TestFixture]
    internal class ConfirmEmailRequestValidatorTests
    {
        private ConfirmEmailRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new ConfirmEmailRequestValidator();
        }

        // Test Case Source
        private static IEnumerable<TestCaseData> ValidationTestCases()
        {
            // Redirect url validation
            yield return new TestCaseData("user@example.com", null, false, "Token")
                .SetDescription("Fails when Token is null.");
            yield return new TestCaseData("user@example.com", "", false, "Token")
                .SetDescription("Fails when Token is empty.");
            yield return new TestCaseData("user@example.com", new string('B', 2049), false, "Token")
                .SetDescription("Fails when Token exceeds maximum length.");

            // Email validation
            yield return new TestCaseData(null, "some-token", false, "Email")
                .SetDescription("Fails when Email is null.");
            yield return new TestCaseData("", "some-token", false, "Email")
                .SetDescription("Fails when Email is empty.");
            yield return new TestCaseData("invalid-email", "some-token", false, "Email")
                .SetDescription("Fails when Email is not a valid email address.");
            yield return new TestCaseData(new string('B', 257) + "@example.com", "some-token", false, "Email")
                .SetDescription("Fails when Email exceeds maximum length.");

            // Valid data
            yield return new TestCaseData("user@example.com", "some-token", true, null)
                .SetDescription("Passes when all fields are valid.");
        }

        [Test]
        [TestCaseSource(nameof(ValidationTestCases))]
        public void Validate_ValidationCases(
            string email,
            string token,
            bool isValid,
            string? errorProperty)
        {
            // Arrange
            var request = new ConfirmEmailRequest
            {
                Email = email,
                Token = token,
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            if (isValid)
            {
                result.ShouldNotHaveAnyValidationErrors();
            }
            else
            {
                Assert.IsNotNull(errorProperty);
                result.ShouldHaveValidationErrorFor(errorProperty!);
            }
        }
    }
}