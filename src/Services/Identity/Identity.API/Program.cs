using Carter;
using Identity.API.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddCarter();

// Identity services
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();

// JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? "SuperSecretKeyForShopMicroservicesIdentityApi_2026!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["JwtSettings:Issuer"]   ?? "ShopMicroservicesIdentityApi",
            ValidAudience            = builder.Configuration["JwtSettings:Audience"] ?? "ShopMicroservicesClients",
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// MassTransit RabbitMQ — publishes UserRegisteredEvent
builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["MessageBroker:Host"] ?? "amqp://guest:guest@localhost:5672", h =>
        {
            h.Username(builder.Configuration["MessageBroker:UserName"] ?? "guest");
            h.Password(builder.Configuration["MessageBroker:Password"] ?? "guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Health checks
builder.Services.AddHealthChecks();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ── Pipeline ──────────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Swagger / OpenAPI documentation
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapCarter();

app.Logger.LogInformation("🚀 Identity.API started — listening on {Urls}", builder.Configuration["ASPNETCORE_URLS"] ?? "http://+:8080");

app.Run();

// Make Program accessible to integration tests (WebApplicationFactory)
public partial class Program { }
