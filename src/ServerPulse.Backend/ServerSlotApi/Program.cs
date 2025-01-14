using Authentication;
using DatabaseControl;
using ExceptionHandling;
using Logging;
using ServerSlotApi;
using ServerSlotApi.Application;
using ServerSlotApi.Infrastructure;
using ServerSlotApi.Infrastructure.Data;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();

builder.AddInfrastructureServices();

builder.Services.AddHttpClientHelperServiceWithResilience(builder.Configuration);

builder.Services.ConfigureIdentityServices(builder.Configuration);

builder.Services.AddAutoMapper(AssemblyReference.Assembly);

builder.Services.AddSharedFluentValidation(typeof(Program), typeof(AssemblyReference));

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