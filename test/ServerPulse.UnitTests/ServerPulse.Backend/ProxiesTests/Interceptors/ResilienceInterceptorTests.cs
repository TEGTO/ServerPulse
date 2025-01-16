using Castle.DynamicProxy;
using Moq;
using Polly;
using Proxies.Attributes;
using System.Diagnostics.CodeAnalysis;
using IInvocation = Castle.DynamicProxy.IInvocation;

namespace Proxies.Interceptors.Tests
{
    [TestFixture]
    internal class ResilienceInterceptorTests
    {
        private ResilienceInterceptor interceptor;

        [SetUp]
        public void Setup()
        {
            interceptor = new ResilienceInterceptor(ResiliencePipeline.Empty);
        }

        [Test]
        public void InterceptSynchronous_InterceptAll_ShouldExecuteThroughPipelineAnyMethod()
        {
            // Arrange
            interceptor = new ResilienceInterceptor(ResiliencePipeline.Empty, interceptAll: true);

            var invocationMock = new Mock<IInvocation>();
            invocationMock.Setup(i => i.Method).Returns(typeof(TestClass).GetMethod(nameof(TestClass.MethodWithoutResilience))!);
            invocationMock.Setup(i => i.Proceed());

            // Act
            interceptor.InterceptSynchronous(invocationMock.Object);

            // Assert
            invocationMock.Verify(i => i.Proceed(), Times.Once);
        }

        [Test]
        public void InterceptAsynchronous_InterceptAll_ShouldExecuteThroughPipelineAnyMethodAsync()
        {
            // Arrange
            interceptor = new ResilienceInterceptor(ResiliencePipeline.Empty, interceptAll: true);
            var invocationMock = new Mock<IInvocation>();
            var invocationProceedInfoMock = new Mock<IInvocationProceedInfo>();
            invocationMock.Setup(i => i.Method).Returns(typeof(TestClass).GetMethod(nameof(TestClass.AsyncMethodWithoutResilience))!);
            invocationMock.Setup(i => i.CaptureProceedInfo()).Returns(() => invocationProceedInfoMock.Object);
            invocationMock.Setup(i => i.ReturnValue).Returns(Task.CompletedTask);
            invocationMock.Setup(i => i.Arguments).Returns([new CancellationToken()]);

            // Act
            interceptor.InterceptAsynchronous(invocationMock.Object);

            // Assert
            invocationProceedInfoMock.Verify(i => i.Invoke(), Times.Once);
        }

        [Test]
        public void InterceptAsynchronous_WithResult_InterceptAll_ShouldReturnPipelineResultForAnyMethodAsync()
        {
            // Arrange
            interceptor = new ResilienceInterceptor(ResiliencePipeline.Empty, interceptAll: true);
            var invocationMock = new Mock<IInvocation>();
            var invocationProceedInfoMock = new Mock<IInvocationProceedInfo>();
            invocationMock.Setup(i => i.Method).Returns(typeof(TestClass).GetMethod(nameof(TestClass.AsyncMethodWithResultWithoutResilience))!);
            invocationMock.Setup(i => i.CaptureProceedInfo()).Returns(() => invocationProceedInfoMock.Object);
            invocationMock.Setup(i => i.ReturnValue).Returns(Task.FromResult(42));
            invocationMock.Setup(i => i.Arguments).Returns([new CancellationToken()]);

            // Act
            interceptor.InterceptAsynchronous<int>(invocationMock.Object);

            // Assert
            invocationProceedInfoMock.Verify(i => i.Invoke(), Times.Once);
        }

        [Test]
        public void InterceptSynchronous_WithResilienceAttribute_ShouldExecuteThroughPipeline()
        {
            // Arrange
            var invocationMock = new Mock<IInvocation>();
            invocationMock.Setup(i => i.Method).Returns(typeof(TestClass).GetMethod(nameof(TestClass.MethodWithResilience))!);
            invocationMock.Setup(i => i.Proceed());

            // Act
            interceptor.InterceptSynchronous(invocationMock.Object);

            // Assert
            invocationMock.Verify(i => i.Proceed(), Times.Once);
        }

        [Test]
        public void InterceptSynchronous_WithoutResilienceAttribute_ShouldProceedDirectly()
        {
            // Arrange
            var invocationMock = new Mock<IInvocation>();
            invocationMock.Setup(i => i.Method).Returns(typeof(TestClass).GetMethod(nameof(TestClass.MethodWithoutResilience))!);
            invocationMock.Setup(i => i.Proceed());

            // Act
            interceptor.InterceptSynchronous(invocationMock.Object);

            // Assert
            invocationMock.Verify(i => i.Proceed(), Times.Once);
        }

        [Test]
        public void InterceptAsynchronous_WithResilienceAttribute_ShouldExecuteThroughPipelineAsync()
        {
            // Arrange
            var invocationMock = new Mock<IInvocation>();
            var invocationProceedInfoMock = new Mock<IInvocationProceedInfo>();
            invocationMock.Setup(i => i.Method).Returns(typeof(TestClass).GetMethod(nameof(TestClass.AsyncMethodWithResilience))!);
            invocationMock.Setup(i => i.CaptureProceedInfo()).Returns(() => invocationProceedInfoMock.Object);
            invocationMock.Setup(i => i.ReturnValue).Returns(Task.CompletedTask);
            invocationMock.Setup(i => i.Arguments).Returns([new CancellationToken()]);

            // Act
            interceptor.InterceptAsynchronous(invocationMock.Object);

            // Assert
            invocationProceedInfoMock.Verify(i => i.Invoke(), Times.Once);
        }

        [Test]
        public void InterceptAsynchronous_WithoutResilienceAttribute_ShouldProceedDirectlyAsync()
        {
            // Arrange
            var invocationMock = new Mock<IInvocation>();
            invocationMock.Setup(i => i.Method).Returns(typeof(TestClass).GetMethod(nameof(TestClass.AsyncMethodWithoutResilience))!);
            invocationMock.Setup(i => i.Proceed());

            // Act
            interceptor.InterceptAsynchronous(invocationMock.Object);

            // Assert
            invocationMock.Verify(i => i.Proceed(), Times.Once);
        }

        [Test]
        public void InterceptAsynchronous_WithResult_ShouldReturnPipelineResultAsync()
        {
            // Arrange
            var invocationMock = new Mock<IInvocation>();
            var invocationProceedInfoMock = new Mock<IInvocationProceedInfo>();
            invocationMock.Setup(i => i.Method).Returns(typeof(TestClass).GetMethod(nameof(TestClass.AsyncMethodWithResult))!);
            invocationMock.Setup(i => i.CaptureProceedInfo()).Returns(() => invocationProceedInfoMock.Object);
            invocationMock.Setup(i => i.ReturnValue).Returns(Task.FromResult(42));
            invocationMock.Setup(i => i.Arguments).Returns([new CancellationToken()]);

            // Act
            interceptor.InterceptAsynchronous<int>(invocationMock.Object);

            // Assert
            invocationProceedInfoMock.Verify(i => i.Invoke(), Times.Once);
        }

        [Test]
        public void InterceptAsynchronous_WithoutResult_ShouldProceedDirectlyAsync()
        {
            // Arrange
            var invocationMock = new Mock<IInvocation>();
            invocationMock.Setup(i => i.Method).Returns(typeof(TestClass).GetMethod(nameof(TestClass.AsyncMethodWithoutResilience))!);
            invocationMock.Setup(i => i.Proceed());

            // Act
            interceptor.InterceptAsynchronous<int>(invocationMock.Object);

            // Assert
            invocationMock.Verify(i => i.Proceed(), Times.Once);
        }
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    [SuppressMessage("Critical Code Smell", "S1186:Methods should not be empty", Justification = "<Pending>")]
    [SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "<Pending>")]
    public class TestClass
    {
        [Resilience]
        public void MethodWithResilience() { }

        public void MethodWithoutResilience() { }

        [Resilience]
        public async Task AsyncMethodWithResilience(CancellationToken cancellationToken) => await Task.CompletedTask;

        public static async Task AsyncMethodWithoutResilience() => await Task.CompletedTask;

        [Resilience]
        public async Task<int> AsyncMethodWithResult(CancellationToken cancellationToken) => await Task.FromResult(42);

        public async Task<int> AsyncMethodWithResultWithoutResilience(CancellationToken cancellationToken) => await Task.FromResult(42);
    }
}