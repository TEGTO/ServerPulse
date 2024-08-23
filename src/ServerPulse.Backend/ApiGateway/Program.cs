using ApiGateway;
using ApiGateway.Middlewares;
using Authentication;
using ConsulUtils.Extension;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;
using Shared.Middlewares;

var builder = WebApplication.CreateBuilder(args);

#region Consul

string environmentName = builder.Environment.EnvironmentName;
builder.Services.AddHealthChecks();
var consulSettings = ConsulExtension.GetConsulSettings(builder.Configuration);
builder.Services.AddConsulService(consulSettings);
builder.Configuration.ConfigureConsul(consulSettings, environmentName);

#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

#region Cors

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var allowedOrigins = builder.Configuration.GetSection(Configuration.ALLOWED_CORS_ORIGINS).Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowCredentials()
        .AllowAnyMethod();
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
        }
    });
});

#endregion


builder.Services.ConfigureIdentityServices(builder.Configuration);

#region Ocelot

var mergedPath = "merged.json";
Utility.MergeJsonFiles(
    [
    "ocelot.json",
    "ocelot.analyzer.json",
    "ocelot.eventprocessing.json",
    "ocelot.authentication.json",
    "ocelot.interaction.json",
    "ocelot.statisticscontrol.json",
    "ocelot.slot.json"
    ], mergedPath);
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(mergedPath, optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddOcelot(builder.Configuration).AddPolly().AddConsul();

#endregion

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);
app.UseExceptionMiddleware();
app.UseMiddleware<TokenFromQueryMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(_ => { });
app.MapHealthChecks("/health");

app.UseOcelotWebSockets();
await app.UseOcelot();
app.Run();