using Microsoft.AspNetCore.Mvc;
using ServerPulse.Client;
using ServerPulse.Client.Services;
using ServerPulse.EventCommunication.Events;

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

        private readonly ILogger<WeatherForecastController> logger;
        private readonly IServerLoadSender serverLoadSender;
        private readonly Configuration configuration;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IServerLoadSender serverLoadSender, Configuration configuration)
        {
            this.logger = logger;
            this.serverLoadSender = serverLoadSender;
            this.configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("manual")]
        public IEnumerable<WeatherForecast> Get_ManualSendEvent()
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
            serverLoadSender.SendEvent(loadEvent);

            return forecast;
        }
    }
}