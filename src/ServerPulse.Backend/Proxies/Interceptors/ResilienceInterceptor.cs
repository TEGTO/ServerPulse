using Castle.DynamicProxy;
using Polly;
using Proxies.Attributes;

namespace Proxies.Interceptors
{
    public class ResilienceInterceptor : IAsyncInterceptor
    {
        private readonly ResiliencePipeline resiliencePipeline;
        private readonly bool interceptAll;

        public ResilienceInterceptor(ResiliencePipeline resiliencePipeline, bool interceptAll = false)
        {
            this.resiliencePipeline = resiliencePipeline;
            this.interceptAll = interceptAll;
        }

        public void InterceptSynchronous(IInvocation invocation)
        {
            if (interceptAll || HasResilienceAttribute(invocation))
            {
                resiliencePipeline.Execute(() =>
                {
                    invocation.Proceed();
                });
            }
            else
            {
                invocation.Proceed();
            }
        }

        public void InterceptAsynchronous(IInvocation invocation)
        {
            if (interceptAll || HasResilienceAttribute(invocation))
            {
                invocation.ReturnValue = InterceptAsync(invocation);
            }
            else
            {
                invocation.Proceed();
            }
        }

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            if (interceptAll || HasResilienceAttribute(invocation))
            {
                invocation.ReturnValue = InterceptAsync<TResult>(invocation);
            }
            else
            {
                invocation.Proceed();
            }
        }

        private async Task InterceptAsync(IInvocation invocation)
        {
            var cancellationToken = GetCancellationToken(invocation);
            var capture = invocation.CaptureProceedInfo();

            await resiliencePipeline.ExecuteAsync(async ct =>
            {
                InjectCancellationTokenIfNeeded(invocation, ct);

                capture.Invoke();

                var task = (Task)invocation.ReturnValue;
                await task;
            }, cancellationToken);
        }

        private async Task<TResult> InterceptAsync<TResult>(IInvocation invocation)
        {
            var cancellationToken = GetCancellationToken(invocation);
            var capture = invocation.CaptureProceedInfo();

            return await resiliencePipeline.ExecuteAsync(async ct =>
            {
                InjectCancellationTokenIfNeeded(invocation, ct);

                capture.Invoke();

                var task = (Task<TResult>)invocation.ReturnValue;
                return await task;
            }, cancellationToken);
        }

        private static bool HasResilienceAttribute(IInvocation invocation)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            return method.GetCustomAttributes(typeof(ResilienceAttribute), true).Any();
        }

        private static CancellationToken GetCancellationToken(IInvocation invocation)
        {
            var cancellationTokenIndex = Array.FindIndex(invocation.Method.GetParameters(), p => p.ParameterType == typeof(CancellationToken));
            return cancellationTokenIndex >= 0 && invocation.Arguments.Length > 0 ? (CancellationToken)invocation.Arguments[cancellationTokenIndex]! : CancellationToken.None;
        }

        private static void InjectCancellationTokenIfNeeded(IInvocation invocation, CancellationToken ct)
        {
            var cancellationTokenIndex = Array.FindIndex(invocation.Method.GetParameters(), p => p.ParameterType == typeof(CancellationToken));
            if (cancellationTokenIndex >= 0 && invocation.Arguments.Length > 0)
            {
                invocation.Arguments[cancellationTokenIndex] = ct;
            }
        }
    }
}
