using Authentication;
using Authentication.Configuration;
using Authentication.Services;
using AuthenticationApi;
using AuthenticationApi.Data;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Services;
using ConsulUtils.Configuration;
using ConsulUtils.Extension;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddDbContextFactory<AuthIdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(Configuration.AUTH_DATABASE_CONNECTION_STRING)));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AuthIdentityDbContext>()
.AddDefaultTokenProviders();

var jwtSettings = new JwtSettings()
{
    Key = builder.Configuration[Configuration.JWT_SETTINGS_KEY],
    Audience = builder.Configuration[Configuration.JWT_SETTINGS_AUDIENCE],
    Issuer = builder.Configuration[Configuration.JWT_SETTINGS_ISSUER],
    ExpiryInMinutes = Convert.ToDouble(builder.Configuration[Configuration.JWT_SETTINGS_EXPIRY_IN_MINUTES]),
};
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddAuthorization();
builder.Services.AddScoped<ITokenHandler, JwtHandler>();
builder.Services.AddCustomJwtAuthentication(jwtSettings);

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDatabaseRepository<AuthIdentityDbContext>, DatabaseRepository<AuthIdentityDbContext>>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
ValidatorOptions.Global.LanguageManager.Enabled = false;

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Configuration[Configuration.EF_CREATE_DATABASE] == "true")
{
    await app.ConfigureDatabaseAsync<AuthIdentityDbContext>(CancellationToken.None);
}

app.UseExceptionMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();