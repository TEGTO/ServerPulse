using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure.Validators;
using FluentValidation.TestHelper;

namespace AuthenticationApi.Validators.Tests
{
    [TestFixture]
    internal class UserUpdateDataRequestValidatorTests
    {
        private UserUpdateDataRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new UserUpdateDataRequestValidator();
        }

        private static IEnumerable<TestCaseData> ValidationTestCases()
        {
            // OldEmail validation
            yield return new TestCaseData(null, null, "ValidOldPass123", null, false, "OldEmail")
                .SetDescription("Fails when OldEmail is null.");
            yield return new TestCaseData("", null, "ValidOldPass123", null, false, "OldEmail")
                .SetDescription("Fails when OldEmail is empty.");
            yield return new TestCaseData("invalid-email", null, "ValidOldPass123", null, false, "OldEmail")
                .SetDescription("Fails when OldEmail is not a valid email address.");
            yield return new TestCaseData(new string('B', 257) + "@example.com", null, "ValidOldPass123", null, false, "OldEmail")
                .SetDescription("Fails when OldEmail exceeds maximum length.");

            // NewEmail validation
            yield return new TestCaseData("old@example.com", "invalid-email", "ValidOldPass123", null, false, "NewEmail")
                .SetDescription("Fails when NewEmail is not a valid email address.");
            yield return new TestCaseData("old@example.com", new string('C', 257) + "@example.com", "ValidOldPass123", null, false, "NewEmail")
                .SetDescription("Fails when NewEmail exceeds maximum length.");
            yield return new TestCaseData("old@example.com", null, "ValidOldPass123", null, true, null)
                .SetDescription("Passes when NewEmail is null or empty.");

            // OldPassword validation
            yield return new TestCaseData("old@example.com", null, null, null, false, "OldPassword")
                .SetDescription("Fails when OldPassword is null.");
            yield return new TestCaseData("old@example.com", null, "", null, false, "OldPassword")
                .SetDescription("Fails when OldPassword is empty.");
            yield return new TestCaseData("old@example.com", null, "short", null, false, "OldPassword")
                .SetDescription("Fails when OldPassword is shorter than the minimum length.");
            yield return new TestCaseData("old@example.com", null, new string('D', 257), null, false, "OldPassword")
                .SetDescription("Fails when OldPassword exceeds maximum length.");

            // NewPassword validation
            yield return new TestCaseData("old@example.com", null, "ValidOldPass123", "short", false, "NewPassword")
                .SetDescription("Fails when NewPassword is shorter than the minimum length.");
            yield return new TestCaseData("old@example.com", null, "ValidOldPass123", new string('E', 257), false, "NewPassword")
                .SetDescription("Fails when NewPassword exceeds maximum length.");
            yield return new TestCaseData("old@example.com", null, "ValidOldPass123", null, true, null)
                .SetDescription("Passes when NewPassword is null or empty.");

            // All fields valid
            yield return new TestCaseData("old@example.com", "new@example.com", "ValidOldPass123", "ValidNewPass123", true, null)
                .SetDescription("Passes when all fields are valid.");
        }

        [Test]
        [TestCaseSource(nameof(ValidationTestCases))]
        public void Validate_ValidationCases(
            string oldEmail,
            string? newEmail,
            string oldPassword,
            string? newPassword,
            bool isValid,
            string? errorProperty)
        {
            // Arrange
            var request = new UserUpdateDataRequest
            {
                OldEmail = oldEmail,
                NewEmail = newEmail,
                OldPassword = oldPassword,
                NewPassword = newPassword
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