using Authentication;
using ConsulUtils.Extension;
using Microsoft.EntityFrameworkCore;
using ServerSlotApi;
using ServerSlotApi.Data;
using ServerSlotApi.Services;
using Shared;
using Shared.Middlewares;
using Shared.Repositories;

var builder = WebApplication.CreateBuilder(args);

string environmentName = builder.Environment.EnvironmentName;
builder.Services.AddHealthChecks();
var consulSettings = ConsulExtension.GetConsulSettings(builder.Configuration);
builder.Services.AddConsulService(consulSettings);
builder.Configuration.ConfigureConsul(consulSettings, environmentName);

builder.Services.AddDbContextFactory<ServerDataDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(Configuration.SERVER_SLOT_DATABASE_CONNECTION_STRING)));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IServerSlotService, ServerSlotService>();
builder.Services.AddScoped<IDatabaseRepository<ServerDataDbContext>, DatabaseRepository<ServerDataDbContext>>();

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

app.UseExceptionMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();