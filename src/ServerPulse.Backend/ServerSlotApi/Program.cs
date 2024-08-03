using Authentication;
using Authentication.Configuration;
using ConsulUtils.Configuration;
using ConsulUtils.Extension;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ServerSlotApi;
using ServerSlotApi.Data;
using ServerSlotApi.Services;
using Shared;
using Shared.Middlewares;
using Shared.Repositories;

var builder = WebApplication.CreateBuilder(args);

var consulConfiguration = new ConsulConfiguration
{
    Host = builder.Configuration[Configuration.CONSUL_HOST]!,
    ServiceName = builder.Configuration[Configuration.CONSUL_SERVICE_NAME]!,
    ServicePort = int.Parse(builder.Configuration[Configuration.CONSUL_SERVICE_PORT]!)
};
string environmentName = builder.Environment.EnvironmentName;
builder.Services.AddHealthChecks();
builder.Services.AddConsulService(consulConfiguration);
builder.Configuration.AddConsulConfiguration(consulConfiguration, environmentName);

builder.Services.AddDbContextFactory<ServerDataDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(Configuration.SERVER_SLOT_DATABASE_CONNECTION_STRING)));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IServerSlotService, ServerSlotService>();
builder.Services.AddScoped<IDatabaseRepository<ServerDataDbContext>, DatabaseRepository<ServerDataDbContext>>();

var jwtSettings = new JwtSettings()
{
    Key = builder.Configuration[Configuration.JWT_SETTINGS_KEY],
    Audience = builder.Configuration[Configuration.JWT_SETTINGS_AUDIENCE],
    Issuer = builder.Configuration[Configuration.JWT_SETTINGS_ISSUER],
    ExpiryInMinutes = Convert.ToDouble(builder.Configuration[Configuration.JWT_SETTINGS_EXPIRY_IN_MINUTES]),
};
builder.Services.AddAuthorization();
builder.Services.AddCustomJwtAuthentication(jwtSettings);

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
ValidatorOptions.Global.LanguageManager.Enabled = false;

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Configuration[Configuration.EF_CREATE_DATABASE] == "true")
{
    await app.ConfigureDatabaseAsync<ServerDataDbContext>(CancellationToken.None);
}

app.UseExceptionMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();