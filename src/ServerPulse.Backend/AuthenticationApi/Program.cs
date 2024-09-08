using Authentication;
using Authentication.Services;
using AuthenticationApi;
using AuthenticationApi.Data;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Services;
using ConsulUtils.Extension;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Middlewares;
using Shared.Repositories;

var builder = WebApplication.CreateBuilder(args);

#region Consul

string environmentName = builder.Environment.EnvironmentName;
builder.Services.AddHealthChecks();
var consulSettings = ConsulExtension.GetConsulSettings(builder.Configuration);
builder.Services.AddConsulService(consulSettings);
builder.Configuration.ConfigureConsul(consulSettings, environmentName);

#endregion

builder.Services.AddDbContextFactory<AuthIdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(Configuration.AUTH_DATABASE_CONNECTION_STRING)));

#region Identity 

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

builder.Services.ConfigureIdentityServices(builder.Configuration);
builder.Services.AddScoped<ITokenHandler, JwtHandler>();

#endregion

#region Project Services 

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IDatabaseRepository<AuthIdentityDbContext>, DatabaseRepository<AuthIdentityDbContext>>();

#endregion

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddSharedFluentValidation(typeof(Program));

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

public partial class Program { }