using Authentication;
using DatabaseControl;
using ExceptionHandling;
using Logging;
using Microsoft.EntityFrameworkCore;
using ServerSlotApi;
using ServerSlotApi.Infrastructure.Data;
using ServerSlotApi.Infrastructure.Repositories;
using ServerSlotApi.Infrastructure.Validators;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.Services.AddDbContextFactory<ServerDataDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(Configuration.SERVER_SLOT_DATABASE_CONNECTION_STRING)));

builder.Services.AddDbContextFactory<ServerDataDbContext>(
    builder.Configuration.GetConnectionString(Configuration.SERVER_SLOT_DATABASE_CONNECTION_STRING)!,
    "ServerSlotApi"
);

builder.Services.AddCustomHttpClientServiceWithResilience(builder.Configuration);

#region Project Services

builder.Services.AddSingleton<IServerSlotRepository, ServerSlotRepository>();

#endregion

builder.Services.AddRepositoryWithResilience<ServerDataDbContext>(builder.Configuration);

builder.Services.ConfigureIdentityServices(builder.Configuration);

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddMediatR(conf =>
{
    conf.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddSharedFluentValidation(typeof(Program), typeof(CheckServerSlotRequestValidator));

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwagger("Server Slot API");
}

var app = builder.Build();

if (app.Configuration[Configuration.EF_CREATE_DATABASE] == "true")
{
    await app.ConfigureDatabaseAsync<ServerDataDbContext>(CancellationToken.None);
}
else
{
    app.UseSwagger("Server Slot API V1");
}

app.UseSharedMiddleware();

app.UseIdentity();

app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync();

public partial class Program { }