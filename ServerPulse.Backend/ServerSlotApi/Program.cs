using Authentication;
using Authentication.Configuration;
using Confluent.Kafka;
using FluentValidation;
using MessageBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ServerSlotApi;
using ServerSlotApi.Data;
using ServerSlotApi.Services;
using Shared;
using Shared.Middlewares;
using Shared.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<ServerDataDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(Configuration.SERVER_SLOT_DATABASE_CONNECTION_STRING)));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IServerSlotService, ServerSlotService>();
builder.Services.AddScoped<IDatabaseRepository<ServerDataDbContext>, DatabaseRepository<ServerDataDbContext>>();

var jwtSettings = new JwtSettings()
{
    Key = builder.Configuration[Configuration.JWT_SETTINGS_KEY],
    Audience = builder.Configuration[Configuration.JWT_SETTINGS_AUDIENCE],
    Issuer = builder.Configuration[Configuration.JWT_SETTINGS_ISSUER],
    ExpiryInMinutes = Convert.ToDouble(builder.Configuration[Configuration.JWT_SETTINGS_EXPIRY_IN_MINUTES]),
};
builder.Services.AddAuthorization();
builder.Services.AddCustomJwtAuthentication(jwtSettings);

builder.Services.AddAutoMapper(typeof(Program).Assembly);

var producerConfig = new ProducerConfig
{
    BootstrapServers = builder.Configuration[Configuration.KAFKA_BOOTSTRAP_SERVERS],
    ClientId = builder.Configuration[Configuration.KAFKA_CLIENT_ID],
    EnableIdempotence = true,

};
builder.Services.AddSingleton<IMessageProducer>(new KafkaProducer(producerConfig));
builder.Services.AddSingleton<ISlotKeyDeletionSender, SlotKeyDeletionSender>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
ValidatorOptions.Global.LanguageManager.Enabled = false;

builder.Services.ConfigureCustomInvalidModelStateResponseControllers();
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


if (app.Configuration[Configuration.EF_CREATE_DATABASE] == "true")
{
    await app.ConfigureDatabaseAsync<ServerDataDbContext>(CancellationToken.None);
}

app.UseHttpsRedirection();
app.UseExceptionMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();