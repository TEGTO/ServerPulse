using ClientUseCase.Middlewares;
using ServerPulse.Client;
using ServerPulse.Client.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var configuration = new ServerPulseSettings
{
    EventController = builder.Configuration["ServerPulse:EventController"],
    Key = builder.Configuration["ServerPulse:Key"],
};
builder.Services.AddServerPulseClient(configuration);

var app = builder.Build();

app.UseExceptionMiddleware();
app.UseLoadMonitor();

app.UseHttpsRedirection();

app.UseAuthorization();


app.MapControllers();

app.Run();