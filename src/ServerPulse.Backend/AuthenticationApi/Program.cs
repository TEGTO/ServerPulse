using Authentication;
using Authentication.Configuration;
using Authentication.Services;
using AuthenticationApi;
using AuthenticationApi.Data;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Middlewares;
using Shared.Repositories;

var builder = WebApplication.CreateBuilder(args);

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

app.UseHttpsRedirection();
app.UseExceptionMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();