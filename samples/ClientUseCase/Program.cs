using ClientUseCase.Middlewares;
using ServerPulse.Client;
using ServerPulse.Client.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var configuration = new SendingSettings
{
    EventServer = builder.Configuration["ServerPulse:EventServer"]!, // Server Pulse API URL
    Key = builder.Configuration["ServerPulse:Key"]!, // Server slot key
};
builder.Services.AddServerPulseClient(configuration);

var app = builder.Build();

app.UseExceptionMiddleware();
app.UseLoadMonitor();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();