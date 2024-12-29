using System.Linq.Expressions;

namespace BackgroundTask
{
    public interface IBackgroundJobClient
    {
        public string Enqueue(Expression<Func<Task>> methodCall);
    }
}