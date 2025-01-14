using AuthenticationApi.Core.Dtos.Endpoints.Auth.UserUpdate;
using FluentValidation.TestHelper;

namespace AuthenticationApi.Application.Validators.Tests
{
    [TestFixture]
    internal class UserUpdateRequestValidatorTests
    {
        private UserUpdateRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new UserUpdateRequestValidator();
        }

        private static IEnumerable<TestCaseData> ValidationTestCases()
        {
            // Email validation
            yield return new TestCaseData(null, "ValidOldPass123", null, false, "Email")
                .SetDescription("Fails when Email is null.");
            yield return new TestCaseData(null, "ValidOldPass123", null, false, "Email")
                .SetDescription("Fails when Email is empty.");
            yield return new TestCaseData("invalid", "ValidOldPass123", null, false, "Email")
                .SetDescription("Fails when Email is not a valid email address.");
            yield return new TestCaseData(new string('B', 257) + "@example.com", "ValidOldPass123", null, false, "Email")
                .SetDescription("Fails when Email exceeds maximum length.");

            // OldPassword validation
            yield return new TestCaseData("valid@email.com", "short", null, false, "OldPassword")
                .SetDescription("Fails when OldPassword is shorter than the minimum length.");
            yield return new TestCaseData("valid@email.com", new string('D', 257), null, false, "OldPassword")
                .SetDescription("Fails when OldPassword exceeds maximum length.");

            // NewPassword validation
            yield return new TestCaseData("valid@email.com", "ValidOldPass123", "short", false, "Password")
                .SetDescription("Fails when NewPassword is shorter than the minimum length.");
            yield return new TestCaseData("valid@email.com", "ValidOldPass123", new string('E', 257), false, "Password")
                .SetDescription("Fails when NewPassword exceeds maximum length.");
            yield return new TestCaseData("valid@email.com", "ValidOldPass123", null, true, null)
                .SetDescription("Passes when NewPassword is null or empty.");

            // All fields valid
            yield return new TestCaseData("new@example.com", "ValidOldPass123", "ValidNewPass123", true, null)
                .SetDescription("Passes when all fields are valid.");
        }

        [Test]
        [TestCaseSource(nameof(ValidationTestCases))]
        public void Validate_ValidationCases(
            string? newEmail,
            string oldPassword,
            string? newPassword,
            bool isValid,
            string? errorProperty)
        {
            // Arrange
            var request = new UserUpdateRequest
            {
                Email = newEmail!,
                OldPassword = oldPassword,
                Password = newPassword
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