using Documentation;
using EventCommunication;
using ExceptionHandling;
using Logging;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ServerMonitorApi;
using ServerMonitorApi.Settings;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.AddInfrastructureServices();

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1 * 1024 * 1024; //1 MB
});

#region Options

var messageBusSettings = builder.Configuration.GetSection(MessageBusSettings.SETTINGS_SECTION).Get<MessageBusSettings>();

ArgumentNullException.ThrowIfNull(messageBusSettings);

builder.Services.Configure<MessageBusSettings>(builder.Configuration.GetSection(MessageBusSettings.SETTINGS_SECTION));

#endregion

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddSharedFluentValidation(typeof(Program), typeof(LoadEvent));

if (builder.Environment.IsDevelopment())
{
    builder.AddDocumentation("Server Monitor API");
}

var app = builder.Build();

app.UseSharedMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger("Server Monitor API V1");
}
else
{
    app.UseHttpsRedirection();
}

app.MapControllers();

await app.RunAsync();

public partial class Program { }