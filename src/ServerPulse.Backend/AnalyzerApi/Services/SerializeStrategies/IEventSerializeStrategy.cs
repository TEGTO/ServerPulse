using AnalyzerApi.Infrastructure.Models.Wrappers;
using MessageBus.Models;

namespace AnalyzerApi.Services.SerializeStrategies
{
    public interface IEventSerializeStrategy<out T> where T : BaseEventWrapper
    {
        public T? SerializeResponse(ConsumeResponse response);
    }
}
