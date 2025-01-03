## Manual Load Event Sending
If you prefer more control over how events are sent, you can send events manually using a custom implementation.

### Inject the Event Sender and Retrieve the Key
Set up your controller to use the event sender and fetch the server slot key from your configuration:
```
public class WeatherForecastController : ControllerBase
{
    private readonly IQueueMessageSender<LoadEvent> loadSender;
    private readonly string key;

    public WeatherForecastController(IQueueMessageSender<LoadEvent> loadSender, IConfiguration configuration)
    {
        this.loadSender = loadSender;
        key = configuration["ServerPulse:Key"]!; // Retrieve the Server Slot Key
    }
}
```

### Send a Load Event with Custom Information
```
[HttpGet]
public IEnumerable<WeatherForecast> Get()
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

    // Create the LoadEvent with relevant details
    var loadEvent = new LoadEvent
    (
        Key: key,
        Endpoint: "/weatherforecast/manual",
        Method: "GET",
        StatusCode: 200,
        Duration: endTime - startTime,
        TimestampUTC: startTime
    );

    // Send the LoadEvent
    loadSender.SendMessage(loadEvent);

    return forecast;
}
```

## Custom Event Sending
For event more control over the events you send and the specific data they contain, you can create and send your own custom events manually.

### Define Your Custom Event
Create a custom event class by inheriting from CustomEvent. Add any additional fields required for your use case:
```
 public record class WeatherForecastControllerGetEvent : CustomEvent
 {
     public WeatherForecastControllerGetEvent(
        string Key, 
        string Name, 
        string Description, 
        DateTime RequestDate, 
        string SerializedRequest)
        : base(Key, Name, Description)
    {
        this.RequestDate = RequestDate;
        this.SerializedRequest = SerializedRequest;
    }
 }
```

### Inject the Event Sender and Retrieve the Key
Set up your controller to use IQueueMessageSender<CustomEventContainer> for sending events. Retrieve the server slot key from your application configuration:
```
public class WeatherForecastController : ControllerBase
{
    private readonly IQueueMessageSender<CustomEventContainer> customSender;
    private readonly string key;

    public WeatherForecastController(
        IQueueMessageSender<CustomEventContainer> customSender, 
        IConfiguration configuration)
    {
        this.customSender = customSender;
        key = configuration["ServerPulse:Key"]!; // Retrieve the Server Slot Key
    }
}
```

### Send a Custom Event 
Use CustomEventContainer to wrap your custom event. Serialize the event and its data for sending:
```
[HttpGet]
public IEnumerable<WeatherForecast> Get()
{
    // Generate forecasts
    var forecasts = Enumerable.Range(1, 5)
        .Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

    // Create a custom event with relevant details
    var weatherForecastEvent = new WeatherForecastControllerGetEvent
    (
        Key: key,
        Name: "WeatherForecastControllerGetEvent",
        Description: "Event shows what forecasts were sent through the API",
        RequestDate: DateTime.UtcNow,
        SerializedRequest: JsonSerializer.Serialize(forecasts)
    );

    // Send the custom event using the sender
    customSender.SendMessage(new CustomEventContainer(
        Event: weatherForecastEvent, 
        SerializedEvent: JsonSerializer.Serialize(weatherForecastEvent)
    ));

    return forecasts;
}
```
### View Your Custom Events on the Info Page

![image](https://github.com/user-attachments/assets/823dc7e2-18db-44ce-8aa3-4b71cd00e75b)

![image](https://github.com/user-attachments/assets/a05af41f-4591-4380-ad67-aa6521b9a284)
