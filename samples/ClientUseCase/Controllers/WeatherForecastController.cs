using ClientUseCase.CustomEvents;
using EventCommunication;
using Microsoft.AspNetCore.Mvc;
using ServerPulse.Client.Services.Interfaces;
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
        private readonly IQueueMessageSender<CustomEventContainer> customSender;
        private readonly string key;

        public WeatherForecastController(IQueueMessageSender<LoadEvent> loadSender, IQueueMessageSender<CustomEventContainer> customSender, IConfiguration configuration)
        {
            this.loadSender = loadSender;
            this.customSender = customSender;
            key = configuration["ServerPulse:Key"]!;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var forecasts = Enumerable.Range(1, 5)
            .Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            var weatherForecastControllerGetEvent = new WeatherForecastControllerGetEvent
            (
                Key: key,
                Name: "WeatherForecastControllerGetEvent",
                Description: "Event shows what forecasts were sent through the API",
                RequestDate: DateTime.UtcNow,
                SerializedRequest: JsonSerializer.Serialize(forecasts)
            );
            customSender.SendMessage(new CustomEventContainer(weatherForecastControllerGetEvent, JsonSerializer.Serialize(weatherForecastControllerGetEvent)));

            return forecasts;
        }

        [HttpGet("manual")]
        public IEnumerable<WeatherForecast> GetManualSendEvent()
        {
            var startTime = DateTime.UtcNow;

            var forecast = Enumerable.Range(1, 5)
            .Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            var endTime = DateTime.UtcNow;

            var loadEvent = new LoadEvent
            (
                Key: key,
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