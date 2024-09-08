using ClientUseCase.CustomEvents;
using Microsoft.AspNetCore.Mvc;
using ServerPulse.Client;
using ServerPulse.Client.Services.Interfaces;
using ServerPulse.EventCommunication;
using ServerPulse.EventCommunication.Events;
using System.Text.Json;

namespace ClientUseCase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IQueueMessageSender<LoadEvent> loadSender;
        private readonly IQueueMessageSender<CustomEventWrapper> customSender;
        private readonly SendingSettings configuration;
        private readonly ILogger<WeatherForecastController> logger;

        public WeatherForecastController(IQueueMessageSender<LoadEvent> loadSender, IQueueMessageSender<CustomEventWrapper> customSender, SendingSettings configuration, ILogger<WeatherForecastController> logger)
        {
            this.loadSender = loadSender;
            this.customSender = customSender;
            this.configuration = configuration;
            this.logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            var weatherForecastControllerGetEvent = new WeatherForecastControllerGetEvent
            (
                Key: configuration.Key,
                Name: "weatherForecastControllerGetEvent",
                Description: "Event shows what forecasts were sent through api",
                RequestDate: DateTime.UtcNow,
                SerializedRequest: JsonSerializer.Serialize(forecasts)
            );
            customSender.SendMessage(new CustomEventWrapper(weatherForecastControllerGetEvent, JsonSerializer.Serialize(weatherForecastControllerGetEvent)));

            return forecasts;
        }

        [HttpGet("manual")]
        public IEnumerable<WeatherForecast> GetManualSendEvent()
        {
            var startTime = DateTime.UtcNow;

            var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            var endTime = DateTime.UtcNow;

            var loadEvent = new LoadEvent
            (
              Key: configuration.Key,
              Endpoint: "/weatherforecast/manual",
              Method: "GET",
              StatusCode: 200,
              Duration: endTime - startTime,
              TimestampUTC: startTime
            );
            loadSender.SendMessage(loadEvent);

            return forecast;
        }
    }
}