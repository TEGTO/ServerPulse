using Authentication;
using Authentication.Token;
using AuthenticationApi;
using AuthenticationApi.Application;
using AuthenticationApi.Core.Entities;
using AuthenticationApi.Infrastructure.Data;
using BackgroundTask;
using DatabaseControl;
using EmailControl;
using ExceptionHandling;
using Hangfire;
using Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Shared;
using InfrastructureKeys = AuthenticationApi.Infrastructure.ConfigurationKeys;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();
builder.Services.AddDbContextFactory<AuthIdentityDbContext>(
    builder.Configuration.GetConnectionString(InfrastructureKeys.AUTH_DATABASE_CONNECTION_STRING)!,
    "AuthenticationApi"
);

builder.Services.AddHttpClientHelperServiceWithResilience(builder.Configuration);
builder.Services.AddFeatureManagement();

builder.AddApplicationServices();
builder.AddInfrastructureServices();

#region Caching

builder.Services.AddStackExchangeRedisCache(
    options =>
    {
        options.Configuration =
            builder.Configuration.GetConnectionString(InfrastructureKeys.REDIS_SERVER_CONNECTION_STRING);
    });

#endregion

#region Identity 

var requireConfirmedEmail = bool.Parse(builder.Configuration[$"FeatureManagement:{Features.EMAIL_CONFIRMATION}"]! ?? "false");

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = requireConfirmedEmail;
})
    .AddEntityFrameworkStores<AuthIdentityDbContext>()
    .AddDefaultTokenProviders();

var isOAuthEnabled = bool.Parse(builder.Configuration[$"FeatureManagement:{Features.OAUTH}"]! ?? "false");
if (isOAuthEnabled)
{
    builder.Services.AddOAuthServices(builder.Configuration);
}

builder.Services.ConfigureIdentityServices(builder.Configuration);
builder.Services.AddScoped<ITokenHandler, JwtHandler>();

#endregion

#region Hanffire

var connectionString = builder.Configuration.GetConnectionString(InfrastructureKeys.AUTH_DATABASE_CONNECTION_STRING);
builder.Services.ConfigureHangfireWthPostgreSql(connectionString);

#endregion

builder.Services.AddRepositoryWithResilience<AuthIdentityDbContext>(builder.Configuration);

builder.Services.AddAutoMapper(AssemblyReference.Assembly);

builder.Services.AddEmailService(builder.Configuration);

builder.Services.AddSharedFluentValidation(typeof(Program), typeof(AssemblyReference));

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwagger("Authentication API");
}

var app = builder.Build();

if (app.Configuration[InfrastructureKeys.EF_CREATE_DATABASE] == "true")
{
    await app.ConfigureDatabaseAsync<AuthIdentityDbContext>(CancellationToken.None);
}

app.UseSharedMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger("Authentication API V1");

    app.UseHangfireDashboard(options: new DashboardOptions()
    {
        Authorization = []
    });
}
else
{
    app.UseHttpsRedirection();
}

app.ConfigureRecurringJobs(app.Configuration);

app.UseIdentity();

app.MapControllers();

await app.RunAsync();

public partial class Program { }