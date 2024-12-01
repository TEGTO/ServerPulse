using Authentication;
using DatabaseControl;
using ExceptionHandling;
using Logging;
using Microsoft.EntityFrameworkCore;
using ServerSlotApi;
using ServerSlotApi.Data;
using ServerSlotApi.Services;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.Services.AddDbContextFactory<ServerDataDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(Configuration.SERVER_SLOT_DATABASE_CONNECTION_STRING)));

builder.Services.AddDbContextFactory<ServerDataDbContext>(
    builder.Configuration.GetConnectionString(Configuration.SERVER_SLOT_DATABASE_CONNECTION_STRING)!,
    "ServerSlotApi");

builder.Services.AddCustomHttpClientServiceWithResilience(builder.Configuration);

#region Project Services

builder.Services.AddSingleton<IServerSlotService, ServerSlotService>();
builder.Services.AddSingleton<ISlotStatisticsService, SlotStatisticsService>();

#endregion

builder.Services.AddRepositoryWithResilience<ServerDataDbContext>(builder.Configuration);

builder.Services.ConfigureIdentityServices(builder.Configuration);

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddSharedFluentValidation(typeof(Program));

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Configuration[Configuration.EF_CREATE_DATABASE] == "true")
{
    await app.ConfigureDatabaseAsync<ServerDataDbContext>(CancellationToken.None);
}

app.UseSharedMiddleware();

app.UseIdentity();

app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync();

public partial class Program { }