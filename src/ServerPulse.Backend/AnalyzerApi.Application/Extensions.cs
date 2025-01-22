using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using EventCommunication;
using MessageBus.Models;
using Shared;

namespace AnalyzerApi.Application
{
    internal static class Extensions
    {
        public static bool TryDeserializeEventWrapper<T, Y>(this ConsumeResponse response, IMapper mapper, out Y? wrapper)
            where T : BaseEvent
            where Y : BaseEventWrapper
        {
            wrapper = null;

            if (response.Message.TryToDeserialize(out T? ev))
            {
                if (ev == null) return false;

                wrapper = mapper.Map<Y>(ev);
                wrapper.CreationDateUTC = response.CreationTimeUTC;
                return true;
            }

            return false;
        }
    }
}