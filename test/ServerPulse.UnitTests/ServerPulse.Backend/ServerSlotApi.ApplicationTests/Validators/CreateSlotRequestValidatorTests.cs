using FluentValidation.TestHelper;
using ServerSlotApi.Core.Dtos.Endpoints.ServerSlot.CreateSlot;

namespace ServerSlotApi.Application.Validators.Tests
{
    [TestFixture]
    public class CreateSlotRequestValidatorTests
    {
        private CreateSlotRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new CreateSlotRequestValidator();
        }

        private static IEnumerable<TestCaseData> ValidationTestCases()
        {
            yield return new TestCaseData(null, false, "Fails when Name is null.")
                .SetDescription("Name cannot be null.");

            yield return new TestCaseData(string.Empty, false, "Fails when Name is empty.")
                .SetDescription("Name cannot be empty.");

            yield return new TestCaseData(new string('A', 257), false, "Fails when Name exceeds maximum length.")
                .SetDescription("Name cannot exceed 256 characters.");

            yield return new TestCaseData(new string('B', 256), true, "Passes when Name is exactly at maximum length.")
                .SetDescription("Name at maximum length is valid.");

            yield return new TestCaseData("ValidName", true, "Passes when Name is valid.")
                .SetDescription("Name with valid length and content is valid.");
        }

        [Test]
        [TestCaseSource(nameof(ValidationTestCases))]
        public void Validate_ValidationTestCases(string name, bool isValid, string description)
        {
            // Arrange
            var request = new CreateSlotRequest { Name = name };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            if (isValid)
            {
                result.ShouldNotHaveAnyValidationErrors();
            }
            else
            {
                result.ShouldHaveValidationErrorFor(x => x.Name);
            }
        }
    }

}