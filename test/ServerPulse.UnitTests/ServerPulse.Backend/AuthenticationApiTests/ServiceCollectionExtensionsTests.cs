using Hangfire;
using Microsoft.AspNetCore.Builder;
using Moq;
using System.Reflection;

namespace AuthenticationApi.Tests
{
    [TestFixture]
    internal class ServiceCollectionExtensionsTests
    {
        private Mock<IApplicationBuilder> appBuilderMock;
        private Mock<IRecurringJobManager> recurringJobManagerMock;

        [SetUp]
        public void SetUp()
        {
            appBuilderMock = new Mock<IApplicationBuilder>();
            recurringJobManagerMock = new Mock<IRecurringJobManager>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(provider => provider.GetService(typeof(IRecurringJobManager)))
                .Returns(recurringJobManagerMock.Object);

            appBuilderMock.Setup(a => a.ApplicationServices).Returns(serviceProviderMock.Object);
        }

        [TestCase(0.1f, "*/6 * * * * *", TestName = "GetCronExpressionForAnyInterval_ValidSecondsInterval")]
        [TestCase(30, "*/30 * * * *", TestName = "GetCronExpressionForAnyInterval_ValidMinutesInterval")]
        [TestCase(120, "0 */2 * * *", TestName = "GetCronExpressionForAnyInterval_ValidHoursInterval")]
        [TestCase(125, "5 */2 * * *", TestName = "GetCronExpressionForAnyInterval_ValidMixedInterval")]
        public void GetCronExpressionForAnyInterval_ValidIntervals_ReturnsCorrectCron(float intervalInMinutes, string expectedCron)
        {
            // Act
            var result = InvokePrivateStaticMethod<string>(
                typeof(ServiceCollectionExtensions),
                "GetCronExpressionForAnyInterval",
                intervalInMinutes);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCron));
        }

        private static T InvokePrivateStaticMethod<T>(Type type, string methodName, params object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                throw new InvalidOperationException($"Method {methodName} not found in {type.Name}.");
            }
            return (T)method.Invoke(null, parameters)!;
        }
    }
}