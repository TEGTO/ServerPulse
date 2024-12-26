using EventCommunication;

namespace ClientUseCase.CustomEvents
{
    public record class WeatherForecastControllerGetEvent : CustomEvent
    {
        public WeatherForecastControllerGetEvent(string Key, string Name, string Description, DateTime RequestDate, string SerializedRequest)
            : base(Key, Name, Description)
        {
        }
    }
}
