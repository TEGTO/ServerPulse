using AnalyzerApi.Core.Models.Wrappers;
using MessageBus.Models;

namespace AnalyzerApi.Application.Services.SerializeStrategies
{
    public interface IEventSerializeStrategy<out T> where T : BaseEventWrapper
    {
        public T? SerializeResponse(ConsumeResponse response);
    }
}
