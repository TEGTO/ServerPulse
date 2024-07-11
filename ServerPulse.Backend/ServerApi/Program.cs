using Authentication;
using Authentication.Configuration;
using Authentication.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ServerApi.Data;
using ServerApi.Services;
using Shared;
using Shared.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ServerDataDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ServerDataConnection")));

var jwtSettings = new JwtSettings()
{
    Key = builder.Configuration["AuthSettings:Key"],
    Audience = builder.Configuration["AuthSettings:Audience"],
    Issuer = builder.Configuration["AuthSettings:Issuer"],
    ExpiryInMinutes = Convert.ToDouble(builder.Configuration["AuthSettings:ExpiryInMinutes"]),
};
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtHandler>();
builder.Services.AddCustomJwtAuthentication(jwtSettings);

builder.Services.AddScoped<IServerSlotService, ServerSlotService>();
builder.Services.AddScoped<IAuthChecker, AuthChecker>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddSharedFluentValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
ValidatorOptions.Global.LanguageManager.Enabled = false;

builder.Services.ConfigureCustomInvalidModelStateResponseContollers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureDatabase<ServerDataDbContext>();

app.UseHttpsRedirection();
app.UseExceptionMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
