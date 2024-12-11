using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExceptionHandling.Tests
{
    [TestFixture]
    internal class ServiceCollectionExtensionsTests
    {
        private IServiceCollection services;
        private ServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
            services = new ServiceCollection();
            services.ConfigureCustomInvalidModelStateResponseControllers();
            serviceProvider = services.BuildServiceProvider();
        }
        [TearDown]
        public void TearDown()
        {
            serviceProvider?.Dispose();
        }

        [Test]
        public void ConfigureCustomInvalidModelStateResponseControllers_InvalidModelState_ShouldThrowValidationException()
        {
            // Arrange
            var context = new ActionContext();

            var modelState = new ModelStateDictionary();

            modelState.AddModelError("TestField", "Test error message");
            context.ModelState.Merge(modelState);

            var apiBehaviorOptions = serviceProvider.GetService<IConfigureOptions<ApiBehaviorOptions>>();
            var options = new ApiBehaviorOptions();

            Assert.That(apiBehaviorOptions, Is.Not.Null);

            apiBehaviorOptions.Configure(options);
            // Act & Assert
            Assert.Throws<ValidationException>(() => options.InvalidModelStateResponseFactory(context));
        }
    }
}