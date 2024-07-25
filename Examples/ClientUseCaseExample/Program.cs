using ServerPulse.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var configuration = new Configuration
{
    EventController = "https://localhost:7129/serverinteraction",
    SlotKey = "13c3bcba-cd71-4736-9aea-774f89fe1ed2",
};

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
