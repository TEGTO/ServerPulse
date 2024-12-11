using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure.Validators;
using FluentValidation.TestHelper;

namespace AuthenticationApi.Validators.Tests
{
    public class AuthTokenValidatorTests
    {
        private AuthTokenValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new AuthTokenValidator();
        }

        private static IEnumerable<TestCaseData> TokenValidationTestCases()
        {
            yield return new TestCaseData(null, "validRefreshToken", false, "AccessToken").SetDescription("AccessToke is null validation should fail.");
            yield return new TestCaseData("", "validRefreshToken", false, "AccessToken").SetDescription("AccessToken is empty validation should fail.");
            yield return new TestCaseData("validAccessToken", null, false, "RefreshToken").SetDescription("RefreshToken is null validation should fail.");
            yield return new TestCaseData("validAccessToken", "", false, "RefreshToken").SetDescription("Refresh token is empty validation should fail.");
            yield return new TestCaseData(new string('A', 2049), "validRefreshToken", false, "AccessToken").SetDescription("Access token is too long validation should fail.");
            yield return new TestCaseData("validAccessToken", new string('B', 2049), false, "RefreshToken").SetDescription("Refresh token is too long validation should fail.");
            yield return new TestCaseData("validAccessToken", "validRefreshToken", true, null).SetDescription("Both tokens is valid validation should pass.");
        }

        [Test]
        [TestCaseSource(nameof(TokenValidationTestCases))]
        public void Validate_TokenValidationCases(string? accessToken, string? refreshToken, bool isValid, string? errorProperty)
        {
            // Arrange
            var request = new AuthToken
            {
                AccessToken = accessToken!,
                RefreshToken = refreshToken!
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