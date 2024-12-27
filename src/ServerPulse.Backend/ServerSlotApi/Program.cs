using Authentication;
using Caching;
using DatabaseControl;
using ExceptionHandling;
using Logging;
using Microsoft.EntityFrameworkCore;
using ServerSlotApi.Dtos;
using ServerSlotApi.Infrastructure;
using ServerSlotApi.Infrastructure.Data;
using ServerSlotApi.Infrastructure.Repositories;
using ServerSlotApi.Infrastructure.Validators;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.Services.AddDbContextFactory<ServerSlotDbContext>(
    builder.Configuration.GetConnectionString(Configuration.SERVER_SLOT_DATABASE_CONNECTION_STRING)!,
    "ServerSlotApi"
);

builder.Services.AddHttpClientHelperServiceWithResilience(builder.Configuration);

#region Project Services

builder.Services.AddSingleton<IServerSlotRepository, ServerSlotRepository>();

#endregion

#region Caching

builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString(Configuration.REDIS_SERVER_CONNECTION_STRING);
});

builder.Services.AddOutputCache((options) =>
{
    options.AddPolicy("BasePolicy", new OutputCachePolicy());

    var expiryTime = int.TryParse(
        builder.Configuration[Configuration.CACHE_GET_BY_EMAIL_SERVER_SLOT_EXPIRY_IN_SECONDS],
        out var getByEmailExpiry) ? getByEmailExpiry : 1;

    options.SetOutputCachePolicy("GetSlotsByEmailPolicy", duration: TimeSpan.FromSeconds(expiryTime), useAuthId: true);

    expiryTime = int.TryParse(
        builder.Configuration[Configuration.CACHE_CHECK_SERVER_SLOT_EXPIRY_IN_SECONDS],
        out var checkSlotExpiry) ? checkSlotExpiry : 1;

    options.SetOutputCachePolicy("CheckSlotKeyPolicy", duration: TimeSpan.FromSeconds(expiryTime), types: typeof(CheckSlotKeyRequest));
});

#endregion

builder.Services.AddRepositoryWithResilience<ServerSlotDbContext>(builder.Configuration);

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
    await app.ConfigureDatabaseAsync<ServerSlotDbContext>(CancellationToken.None);
}
else
{
    app.UseSwagger("Server Slot API V1");
}

app.UseSharedMiddleware();

app.UseIdentity();

app.UseOutputCache(); //Order after Identity

app.MapControllers();

await app.RunAsync();

public partial class Program { }