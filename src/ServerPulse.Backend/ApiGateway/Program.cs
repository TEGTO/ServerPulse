using ApiGateway;
using ApiGateway.Middlewares;
using Authentication;
using ExceptionHandling;
using Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

#region Cors

bool.TryParse(builder.Configuration[ApiGateway.ConfigurationKeys.USE_CORS], out bool useCors);
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

if (useCors)
{
    builder.Services.AddApplicationCors(builder.Configuration, myAllowSpecificOrigins, builder.Environment.IsDevelopment());
}

#endregion

builder.Services.AddIdentity(builder.Configuration);
builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

#region Ocelot

var env = builder.Environment.EnvironmentName;

var mergedPath = $"merged.{env}.json";

Utility.MergeJsonFiles(
    [
        $"ocelot.{env}.json",
        $"ocelot.{env}.analyzer.json",
        $"ocelot.{env}.authentication.json",
        $"ocelot.{env}.oauth.json",
        $"ocelot.{env}.interaction.json",
        $"ocelot.{env}.slot.json",
        $"ocelot.{env}.swagger.json",
    ], mergedPath);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(mergedPath, optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddOcelot(builder.Configuration).AddPolly();

#endregion

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerForOcelot(builder.Configuration);
}

var app = builder.Build();

if (useCors)
{
    app.UseCors(myAllowSpecificOrigins);
}

app.UseSharedMiddleware();
app.UseMiddleware<TokenFromQueryMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseSwaggerForOcelotUI(opt =>
    {
        opt.PathToSwaggerGenerator = "/swagger/docs";
        opt.DownstreamSwaggerHeaders =
        [
              new KeyValuePair<string, string>("Authorization", "Bearer {token}")
        ];
    });
}

app.UseEndpoints(_ => { });
app.MapHealthChecks("/health");

app.UseOcelotWebSockets();
await app.UseOcelot();
await app.RunAsync();

public partial class Program { }