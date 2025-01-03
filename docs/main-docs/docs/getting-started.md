## Create a Server Slot and Retrieve the Key
To start sending metrics to Server Pulse, you first need to create a server slot and obtain the corresponding key. The key uniquely identifies your server slot and is required to connect your service to Server Pulse.

![image](https://github.com/user-attachments/assets/e4f1c683-3aa8-495a-ae07-47afdc2eb864)

![image](https://github.com/user-attachments/assets/9be696f1-b24d-4267-aa2e-714aa04a3e1b)

## Install the Library
Add the [Server Pulse](https://www.nuget.org/packages/TEGTO.ServerPulse.Client) NuGet package to your project:
```
<PackageReference Include="TEGTO.ServerPulse.Client" Version="1.0.0" />
```
Alternatively, you can install it using the .NET CLI:
```
dotnet add package TEGTO.ServerPulse.Client --version 1.0.0
```

## Setup Configuration Settings
Update your application settings to include the EventServer URL and Key:
```
var settings = new SendingSettings
{
    EventServer = builder.Configuration["ServerPulse:EventServer"]!, // Server Pulse API Gateway URL
    Key = builder.Configuration["ServerPulse:Key"]!, // Server Slot Key
};

builder.Services.AddServerPulseClient(settings);
```
> EventServer: The Server Pulse API Gateway URL, e.g., https://localhost:7129.

> Key: Obtain the key from the Server Pulse frontend or the Server Slot API.

## Add Middleware
Enable HTTP metrics collection by adding the following middleware:
```
app.UseLoadMonitor();
```
## Go to frontend to see the metrics. 
Head over to the Server Pulse frontend to monitor your metrics in real time, thanks to SignalR's live updates.

For additional insights, navigate to the Info Page, where you can access detailed metrics and analytics.

![image](https://github.com/user-attachments/assets/40b102ca-2432-4f24-a21d-66152a969b60)

![image](https://github.com/user-attachments/assets/ecf25246-3b1a-4823-bac6-d7e10a13e0e5)

## Advanced Configuration (Optional)
You can override the default sending settings:
```
   public class SendingSettings
{
    /// The Server Slot key. Required to link metrics to Server Pulse.
    public required string Key { get; set; }

    /// The Server Pulse API Gateway URL or Server Monitor API URL for local setups.
    public required string EventServer { get; set; } = default!;

    /// The frequency (in seconds) for sending pulse events. 
    /// Backend default is one message every 3 seconds.
    public double ServerKeepAliveInterval { get; set; } = 10d;

    /// Maximum number of events per message (excluding pulse events). 
    /// Backend default is 99 messages.
    public int MaxEventSendingAmount { get; set; } = 10;

    /// Frequency (in seconds) for sending event data messages (excluding pulse events). 
    /// Backend default is one message every 3 seconds.
    public double SendingInterval { get; set; } = 15d;
}
```