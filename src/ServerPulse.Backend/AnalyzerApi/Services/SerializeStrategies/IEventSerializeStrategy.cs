using AnalyzerApi.Infrastructure.Models.Wrappers;
using MessageBus.Interfaces;

namespace AnalyzerApi.Services.SerializeStrategies
{
    public interface IEventSerializeStrategy<out T> where T : BaseEventWrapper
    {
        public T? SerializeResponse(ConsumeResponse response);
    }
}
