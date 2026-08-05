using Carter;
using MassTransit;
using Notification.API.EventHandlers;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddCarter();

// MongoDB Registration for Notification Audit Trail
var mongoConnString = builder.Configuration["DatabaseSettings:ConnectionString"]
    ?? builder.Configuration.GetConnectionString("NotificationDb");

if (!string.IsNullOrEmpty(mongoConnString))
{
    var mongoUrl = new MongoDB.Driver.MongoUrl(mongoConnString);
    var mongoClient = new MongoDB.Driver.MongoClient(mongoUrl);
    var databaseName = mongoUrl.DatabaseName ?? "notificationdb";
    builder.Services.AddSingleton<MongoDB.Driver.IMongoDatabase>(_ => mongoClient.GetDatabase(databaseName));
    builder.Services.AddSingleton<Notification.API.Services.INotificationRepository, Notification.API.Services.MongoNotificationRepository>();
}

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
    // Swagger / OpenAPI documentation
}

app.MapHealthChecks("/health");

// Status endpoint
app.MapGet("/api/notifications/status", () => Results.Ok(new
{
    Service   = "Notification.API",
    Status    = "Running",
    Consumers = new[]
    {
        "UserRegisteredEventHandler  → Welcome email (Logged to MongoDB)",
        "CartCheckoutEventHandler    → Order confirmation email + SMS (Logged to MongoDB)"
    },
    Timestamp = DateTime.UtcNow
}))
.WithTags("Notification")
.WithName("NotificationStatus")
.WithSummary("Notification service status and consumer list");

// Query notification audit logs from MongoDB
app.MapGet("/api/notifications/logs", async (Notification.API.Services.INotificationRepository? repo) =>
{
    if (repo is null)
    {
        return Results.Ok(new { Message = "MongoDB storage not configured for notification logs." });
    }

    var logs = await repo.GetRecentNotificationsAsync();
    return Results.Ok(logs);
})
.WithTags("Notification")
.WithName("GetNotificationLogs")
.WithSummary("Get recent notification audit logs from MongoDB");

app.MapCarter();

app.Logger.LogInformation("🔔 Notification.API started — listening on {Urls}", builder.Configuration["ASPNETCORE_URLS"] ?? "http://+:8080");

app.Run();
