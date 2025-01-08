using Authentication;
using Caching;
using DatabaseControl;
using ExceptionHandling;
using Logging;
using Microsoft.EntityFrameworkCore;
using ServerSlotApi.Dtos.Endpoints.Slot.CheckSlotKey;
using ServerSlotApi.Infrastructure.Configuration;
using ServerSlotApi.Infrastructure.Data;
using ServerSlotApi.Infrastructure.Repositories;
using ServerSlotApi.Infrastructure.Validators;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.Services.AddDbContextFactory<ServerSlotDbContext>(
    builder.Configuration.GetConnectionString(ConfigurationKeys.SERVER_SLOT_DATABASE_CONNECTION_STRING)!,
    "ServerSlotApi"
);

builder.Services.AddHttpClientHelperServiceWithResilience(builder.Configuration);

#region Options

var cacheSettings = builder.Configuration.GetSection(CacheSettings.SETTINGS_SECTION).Get<CacheSettings>();

ArgumentNullException.ThrowIfNull(cacheSettings);

builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection(CacheSettings.SETTINGS_SECTION));

#endregion

#region Project Services

builder.Services.AddSingleton<IServerSlotRepository, ServerSlotRepository>();

#endregion

#region Caching

builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString(CacheSettings.REDIS_SERVER_CONNECTION_STRING);
});

builder.Services.AddOutputCache((options) =>
{
    options.AddPolicy("BasePolicy", new OutputCachePolicy());

    var expiryTime = cacheSettings.GetServerSlotByEmailExpiryInSeconds;

    options.SetOutputCachePolicy("GetSlotsByEmailPolicy", duration: TimeSpan.FromSeconds(expiryTime), useAuthId: true);

    expiryTime = cacheSettings.ServerSlotCheckExpiryInSeconds;

    options.SetOutputCachePolicy("CheckSlotKeyPolicy", duration: TimeSpan.FromSeconds(expiryTime), types: typeof(CheckSlotKeyRequest));
});

#endregion

builder.Services.AddRepositoryWithResilience<ServerSlotDbContext>(builder.Configuration);

builder.Services.ConfigureIdentityServices(builder.Configuration);

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddSharedFluentValidation(typeof(Program), typeof(CheckSlotKeyRequestValidator));

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwagger("Server Slot API");
}

var app = builder.Build();

app.UseSharedMiddleware();

if (app.Configuration[ConfigurationKeys.EF_CREATE_DATABASE] == "true")
{
    await app.ConfigureDatabaseAsync<ServerSlotDbContext>(CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger("Server Monitor API V1");
}
else
{
    app.UseHttpsRedirection();
}

app.UseIdentity();

app.UseOutputCache(); //Order after Identity

app.MapControllers();

await app.RunAsync();

public partial class Program { }