using Carter;
using MassTransit;
using Notification.API.EventHandlers;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddCarter();
builder.Services.AddOpenApi();

// MassTransit RabbitMQ — subscribes to all notification events
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<UserRegisteredEventHandler>();
    config.AddConsumer<CartCheckoutEventHandler>();

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["MessageBroker:Host"] ?? "amqp://guest:guest@localhost:5672", h =>
        {
            h.Username(builder.Configuration["MessageBroker:UserName"] ?? "guest");
            h.Password(builder.Configuration["MessageBroker:Password"] ?? "guest");
        });

        // Explicit receive endpoints for reliability
        cfg.ReceiveEndpoint("user-registered-notification-queue", e =>
        {
            e.ConfigureConsumer<UserRegisteredEventHandler>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        });

        cfg.ReceiveEndpoint("cart-checkout-notification-queue", e =>
        {
            e.ConfigureConsumer<CartCheckoutEventHandler>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
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
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

// Status endpoint
app.MapGet("/api/notifications/status", () => Results.Ok(new
{
    Service   = "Notification.API",
    Status    = "Running",
    Consumers = new[]
    {
        "UserRegisteredEventHandler  → Welcome email",
        "CartCheckoutEventHandler    → Order confirmation email + SMS"
    },
    Timestamp = DateTime.UtcNow
}))
.WithTags("Notification")
.WithName("NotificationStatus")
.WithSummary("Notification service status and consumer list");

app.MapCarter();

app.Logger.LogInformation("🔔 Notification.API started — listening on {Urls}", builder.Configuration["ASPNETCORE_URLS"] ?? "http://+:8080");

app.Run();
