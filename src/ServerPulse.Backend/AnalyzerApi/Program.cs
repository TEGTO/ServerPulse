using AnalyzerApi;
using Documentation;
using ExceptionHandling;
using Logging;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.AddApplicationServices();
builder.AddInfrastructureServices();

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();

if (builder.Environment.IsDevelopment())
{
    builder.AddDocumentation("Analyzer API");
}

var app = builder.Build();

app.UseSharedMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger("Analyzer API V1");
}
else
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.UseOutputCache(); //Order after Identity

app.MapHubs();

app.MapHealthChecks("/health");

await app.RunAsync();

public partial class Program { }