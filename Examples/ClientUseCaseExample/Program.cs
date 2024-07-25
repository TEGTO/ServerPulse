using ServerPulse.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var configuration = new Configuration
{
    EventController = "https://localhost:7129",
    SlotKey = "13c3bcba-cd71-4736-9aea-774f89fe1ed2",
};
builder.Services.AddServerPulseClient(configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
