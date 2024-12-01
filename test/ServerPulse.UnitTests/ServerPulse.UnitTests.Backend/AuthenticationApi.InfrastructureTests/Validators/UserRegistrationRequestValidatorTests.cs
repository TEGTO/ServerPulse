using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure.Validators;
using FluentValidation.TestHelper;

namespace AuthenticationApi.Validators.Tests
{
    [TestFixture]
    internal class UserRegistrationRequestValidatorTests
    {
        private UserRegistrationRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new UserRegistrationRequestValidator();
        }

        // Test Case Source
        private static IEnumerable<TestCaseData> ValidationTestCases()
        {
            // UserName validation
            yield return new TestCaseData(null, "user@example.com", "ValidPassword123", "ValidPassword123", false, "UserName")
                .SetDescription("Fails when UserName is null.");
            yield return new TestCaseData("", "user@example.com", "ValidPassword123", "ValidPassword123", false, "UserName")
                .SetDescription("Fails when UserName is empty.");
            yield return new TestCaseData(new string('A', 257), "user@example.com", "ValidPassword123", "ValidPassword123", false, "UserName")
                .SetDescription("Fails when UserName exceeds maximum length.");

            // Email validation
            yield return new TestCaseData("User", null, "ValidPassword123", "ValidPassword123", false, "Email")
                .SetDescription("Fails when Email is null.");
            yield return new TestCaseData("User", "", "ValidPassword123", "ValidPassword123", false, "Email")
                .SetDescription("Fails when Email is empty.");
            yield return new TestCaseData("User", "invalid-email", "ValidPassword123", "ValidPassword123", false, "Email")
                .SetDescription("Fails when Email is not a valid email address.");
            yield return new TestCaseData("User", new string('B', 257) + "@example.com", "ValidPassword123", "ValidPassword123", false, "Email")
                .SetDescription("Fails when Email exceeds maximum length.");

            // Password validation
            yield return new TestCaseData("User", "user@example.com", null, "ValidPassword123", false, "Password")
                .SetDescription("Fails when Password is null.");
            yield return new TestCaseData("User", "user@example.com", "", "ValidPassword123", false, "Password")
                .SetDescription("Fails when Password is empty.");
            yield return new TestCaseData("User", "user@example.com", "12345", "12345", false, "Password")
                .SetDescription("Fails when Password is shorter than the minimum length.");
            yield return new TestCaseData("User", "user@example.com", new string('C', 257), new string('C', 257), false, "Password")
                .SetDescription("Fails when Password exceeds maximum length.");

            // ConfirmPassword validation
            yield return new TestCaseData("User", "user@example.com", "ValidPassword123", null, false, "ConfirmPassword")
                .SetDescription("Fails when ConfirmPassword is null.");
            yield return new TestCaseData("User", "user@example.com", "ValidPassword123", "", false, "ConfirmPassword")
                .SetDescription("Fails when ConfirmPassword is empty.");
            yield return new TestCaseData("User", "user@example.com", "ValidPassword123", "MismatchedPassword", false, "ConfirmPassword")
                .SetDescription("Fails when ConfirmPassword does not match Password.");

            // Valid data
            yield return new TestCaseData("User", "user@example.com", "ValidPassword123", "ValidPassword123", true, null)
                .SetDescription("Passes when all fields are valid.");
        }

        [Test]
        [TestCaseSource(nameof(ValidationTestCases))]
        public void Validate_ValidationCases(
            string userName,
            string email,
            string password,
            string confirmPassword,
            bool isValid,
            string? errorProperty)
        {
            // Arrange
            var request = new UserRegistrationRequest
            {
                UserName = userName,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
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
