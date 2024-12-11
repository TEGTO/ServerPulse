using AnalyzerApi.Infrastructure.Wrappers;
using AutoMapper;
using EventCommunication.Events;
using MessageBus.Interfaces;
using Shared;

namespace AnalyzerApi
{
    public static class Extensions
    {
        public static bool TryDeserializeEventWrapper<T, Y>(this ConsumeResponse response, IMapper mapper, out Y wrapper)
            where T : BaseEvent
            where Y : BaseEventWrapper
        {
            if (response.Message.TryToDeserialize(out T ev))
            {
                wrapper = mapper.Map<Y>(ev);
                wrapper.CreationDateUTC = response.CreationTimeUTC;
                return true;
            }
            wrapper = null;
            return false;
        }
    }
}