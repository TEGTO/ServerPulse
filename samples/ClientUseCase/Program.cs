using ClientUseCase.Middlewares;
using ServerPulse.Client;
using ServerPulse.Client.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var configuration = new SendingSettings
{
    Key = builder.Configuration["ServerPulse:Key"]!, // Server slot key
    EventServer = builder.Configuration["ServerPulse:EventServer"]!, // Server Pulse API URL
};
builder.Services.AddServerPulseClient(configuration);

var app = builder.Build();

app.UseExceptionMiddleware();
app.UseLoadMonitor();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();