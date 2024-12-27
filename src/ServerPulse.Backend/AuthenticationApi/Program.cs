using Authentication;
using Authentication.OAuth.Google;
using Authentication.Token;
using AuthenticationApi;
using AuthenticationApi.Dtos.OAuth;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Infrastructure.Data;
using AuthenticationApi.Infrastructure.Validators;
using AuthenticationApi.Services;
using DatabaseControl;
using ExceptionHandling;
using Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogging();
builder.Services.AddDbContextFactory<AuthIdentityDbContext>(
    builder.Configuration.GetConnectionString(ConfigurationKeys.AUTH_DATABASE_CONNECTION_STRING)!,
    "AuthenticationApi"
);

builder.Services.AddHttpClientHelperServiceWithResilience(builder.Configuration);

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
builder.Services.AddOAuthServices(builder.Configuration);
builder.Services.AddScoped<ITokenHandler, JwtHandler>();

#endregion

#region Project Services 

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<GoogleOAuthService>();
builder.Services.AddScoped<IGoogleOAuthHttpClient, GoogleOAuthHttpClient>();
builder.Services.AddScoped(provider => new Dictionary<OAuthLoginProvider, IOAuthService>
    {
        { OAuthLoginProvider.Google, provider.GetService<GoogleOAuthService>()! },
    });

#endregion

builder.Services.AddRepositoryWithResilience<AuthIdentityDbContext>(builder.Configuration);

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddMediatR(conf =>
{
    conf.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddSharedFluentValidation(typeof(Program), typeof(AccessTokenDataDtoValidator));

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwagger("Authentication API");
}

var app = builder.Build();

if (app.Configuration[ConfigurationKeys.EF_CREATE_DATABASE] == "true")
{
    await app.ConfigureDatabaseAsync<AuthIdentityDbContext>(CancellationToken.None);
}

app.UseSharedMiddleware();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
else
{
    app.UseSwagger("Authentication API V1");
}

app.UseIdentity();

app.MapControllers();

await app.RunAsync();

public partial class Program { }