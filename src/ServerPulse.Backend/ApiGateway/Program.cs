using ApiGateway;
using Authentication;
using Authentication.Configuration;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Shared.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var allowedOrigins = builder.Configuration.GetSection("AllowedCORSOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod();
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
        }
    });
});

var jwtSettings = new JwtSettings()
{
    Key = builder.Configuration["AuthSettings:Key"],
    Audience = builder.Configuration["AuthSettings:Audience"],
    Issuer = builder.Configuration["AuthSettings:Issuer"],
    ExpiryInMinutes = Convert.ToDouble(builder.Configuration["AuthSettings:ExpiryInMinutes"]),
};
builder.Services.AddCustomJwtAuthentication(jwtSettings);

var mergedPath = "merged.json";
Utility.MergeJsonFiles(
    [
    "ocelot.json",
    "ocelot.analyzer.json",
    "ocelot.authentication.json",
    "ocelot.interaction.json",
    "ocelot.slot.json"
    ], mergedPath);
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(mergedPath, optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddOcelot(builder.Configuration).AddPolly();

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);
app.UseExceptionMiddleware();

app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();
app.Run();