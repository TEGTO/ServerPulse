using Hangfire;
using System.Linq.Expressions;

namespace BackgroundTask
{
    public class HangfireBackgroundJobClient : IBackgroundJobClient
    {
        public string Enqueue(Expression<Func<Task>> methodCall)
        {
            return BackgroundJob.Enqueue(methodCall);
        }
    }
}
