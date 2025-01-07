using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.RefreshToken;
using FluentValidation.TestHelper;

namespace UserApi.Validators.Tests
{
    [TestFixture]
    internal class RefreshTokenRequestValidatorTests
    {
        private RefreshTokenRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new RefreshTokenRequestValidator();
        }

        private static IEnumerable<TestCaseData> TokenValidationTestCases()
        {
            yield return new TestCaseData(null, "validRefreshToken", false, "AccessToken").SetDescription("AccessToken is null validation should fail.");
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
            var request = new RefreshTokenRequest
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