using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Login;
using AuthenticationApi.Infrastructure.Validators;
using FluentValidation.TestHelper;

namespace AuthenticationApi.Validators.Tests
{
    [TestFixture]
    internal class LoginRequestValidatorTests
    {
        private LoginRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new LoginRequestValidator();
        }

        private static IEnumerable<TestCaseData> ValidationTestCases()
        {
            yield return new TestCaseData(null, "validPassword", false, "Login")
                .SetDescription("Fails validation when Login is null.");

            yield return new TestCaseData("", "validPassword", false, "Login")
                .SetDescription("Fails validation when Login is empty.");

            yield return new TestCaseData(new string('A', 257), "validPassword", false, "Login")
                .SetDescription("Fails validation when Login exceeds maximum length.");

            yield return new TestCaseData("validLogin", null, false, "Password")
                .SetDescription("Fails validation when Password is null.");

            yield return new TestCaseData("validLogin", "", false, "Password")
                .SetDescription("Fails validation when Password is empty.");

            yield return new TestCaseData("validLogin", "1234567", false, "Password")
                .SetDescription("Fails validation when Password is shorter than the minimum length.");

            yield return new TestCaseData("validLogin", new string('B', 257), false, "Password")
                .SetDescription("Fails validation when Password exceeds maximum length.");

            yield return new TestCaseData("validLogin", "validPassword", true, null)
                .SetDescription("Passes validation when both Login and Password are valid.");
        }

        [Test]
        [TestCaseSource(nameof(ValidationTestCases))]
        public void Validate_ValidationCases(string login, string password, bool isValid, string? errorProperty)
        {
            // Arrange
            var request = new LoginRequest
            {
                Login = login,
                Password = password
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
