using FluentValidation.TestHelper;
using ServerSlotApi.Dtos.Endpoints.Slot.UpdateSlot;

namespace ServerSlotApi.Infrastructure.Validators.Tests
{
    [TestFixture]
    internal class UpdateSlotRequestValidatorTests
    {
        private UpdateSlotRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new UpdateSlotRequestValidator();
        }

        private static IEnumerable<TestCaseData> ValidationTestCases()
        {
            yield return new TestCaseData(null, "ValidName", false, "Fails when Id is null.")
                .SetDescription("Id cannot be null.");

            yield return new TestCaseData(string.Empty, "ValidName", false, "Fails when Id is empty.")
                .SetDescription("Id cannot be empty.");

            yield return new TestCaseData(new string('A', 257), "ValidName", false, "Fails when Id exceeds maximum length.")
                .SetDescription("Id cannot exceed 256 characters.");

            yield return new TestCaseData(new string('B', 256), "ValidName", true, "Passes when Id is exactly at maximum length.")
                .SetDescription("Id at maximum length is valid.");

            yield return new TestCaseData("ValidId", null, false, "Fails when Name is null.")
                .SetDescription("Name cannot be null.");

            yield return new TestCaseData("ValidId", string.Empty, false, "Fails when Name is empty.")
                .SetDescription("Name cannot be empty.");

            yield return new TestCaseData("ValidId", new string('A', 257), false, "Fails when Name exceeds maximum length.")
                .SetDescription("Name cannot exceed 256 characters.");

            yield return new TestCaseData("ValidId", new string('B', 256), true, "Passes when Name is exactly at maximum length.")
                .SetDescription("Name at maximum length is valid.");

            yield return new TestCaseData("ValidId", "ValidName", true, "Passes when Id and Name are valid.")
                .SetDescription("Both Id and Name are valid.");
        }

        [Test]
        [TestCaseSource(nameof(ValidationTestCases))]
        public void Validate_ValidationTestCases(string id, string name, bool isValid, string description)
        {
            // Arrange
            var request = new UpdateSlotRequest { Id = id, Name = name };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            if (isValid)
            {
                result.ShouldNotHaveAnyValidationErrors();
            }
            else
            {
                if (id == null || string.IsNullOrEmpty(id) || id.Length > 256)
                {
                    result.ShouldHaveValidationErrorFor(x => x.Id);
                }
                if (name == null || string.IsNullOrEmpty(name) || name.Length > 256)
                {
                    result.ShouldHaveValidationErrorFor(x => x.Name);
                }
            }
        }

    }
}