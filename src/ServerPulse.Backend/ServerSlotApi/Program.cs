using Authentication;
using DatabaseControl;
using Documentation;
using ExceptionHandling;
using Logging;
using ServerSlotApi;
using ServerSlotApi.Application;
using ServerSlotApi.Infrastructure.Data;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.AddInfrastructureServices();

builder.Services.AddIdentity(builder.Configuration);

builder.Services.AddAutoMapper(AssemblyReference.Assembly);

builder.Services.AddSharedFluentValidation(typeof(Program), typeof(AssemblyReference));

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsDevelopment())
{
    builder.AddDocumentation("Server Slot API");
}

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSharedMiddleware();

if (app.Configuration[ServerSlotApi.Infrastructure.ConfigurationKeys.EF_CREATE_DATABASE]?.ToLower() == "true")
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

app.MapHealthChecks("/health");

await app.RunAsync();

public partial class Program { }