using ApiGateway;
using Authentication;
using ConsulUtils.Configuration;
using ConsulUtils.Extension;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;
using Shared.Middlewares;

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

builder.Services.ConfigureIdentityServices(builder.Configuration);

var mergedPath = "merged.json";
Utility.MergeJsonFiles(
    [
    "ocelot.json",
    "ocelot.analyzer.json",
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

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);
app.UseExceptionMiddleware();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(_ => { });
app.MapHealthChecks("/health");

app.UseWebSockets();
await app.UseOcelot();
app.Run();