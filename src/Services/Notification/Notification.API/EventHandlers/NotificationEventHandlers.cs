using BuildingBlocks.Messaging.Events;
using MassTransit;
using Notification.API.Models;
using Notification.API.Services;

namespace Notification.API.EventHandlers;

// ── User Registered → Welcome Email + MongoDB Log ─────────────────────────────

public class UserRegisteredEventHandler(
    ILogger<UserRegisteredEventHandler> logger,
    INotificationRepository? repo = null)
    : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation(
            "📧 [WELCOME EMAIL] Sending to {Email} — Welcome, {FirstName} {LastName}! (UserId: {UserId})",
            evt.Email, evt.FirstName, evt.LastName, evt.UserId);

        var subject = $"Welcome to ShopMicroservices, {evt.FirstName}!";
        var body = $"""
                    Hi {evt.FirstName} {evt.LastName},

                    Your account has been created successfully.
                    User ID : {evt.UserId}

                    Start shopping now at https://shop.example.com

                    Regards,
                    The Shop Team
                    """;

        await SimulateEmailAsync(evt.Email, subject, body);

        // Persist notification log to MongoDB — idempotent upsert on EventId+Channel.
        // Wrapped in try/catch: a DB failure must NOT cause the handler to throw,
        // because that would trigger MassTransit retry and re-send the welcome email.
        if (repo is not null)
        {
            try
            {
                await repo.LogNotificationAsync(new NotificationLog
                {
                    EventId   = evt.Id,
                    EventType = "UserRegistered",
                    Recipient = evt.Email,
                    Channel   = "Email",
                    Subject   = subject,
                    Message   = body,
                    Status    = "Sent",
                    Metadata  = new Dictionary<string, string>
                    {
                        { "UserId",     evt.UserId },
                        { "FirstName",  evt.FirstName },
                        { "LastName",   evt.LastName }
                    }
                });
            }
            catch (MongoDB.Driver.MongoException ex)
            {
                // Log but do not rethrow — database audit failure must not duplicate the notification.
                logger.LogWarning(ex, "⚠️ MongoDB failure persisting notification log for event {EventId}", evt.Id);
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TimeoutException ex)
            {
                logger.LogWarning(ex, "⚠️ Timed out while persisting notification log for event {EventId}", evt.Id);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "⚠️ Invalid operation persisting notification log for event {EventId}", evt.Id);
            }
        }

        logger.LogInformation("✅ [WELCOME EMAIL] Successfully dispatched and logged for {Email}", evt.Email);
    }

    private static async Task SimulateEmailAsync(string to, string subject, string body)
    {
        await Task.Delay(50);
    }
}

// ── Cart Checkout → Order Confirmation Email + SMS + MongoDB Log ──────────────

public class CartCheckoutEventHandler(
    ILogger<CartCheckoutEventHandler> logger,
    INotificationRepository? repo = null)
    : IConsumer<CartCheckoutEvent>
{
    public async Task Consume(ConsumeContext<CartCheckoutEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation(
            "🛒 [ORDER CONFIRMATION] Sending to {Email} — Order for {UserName}, Total: {Total:C}",
            evt.EmailAddress, evt.UserName, evt.TotalPrice);

        var subject = "Order Confirmation — ShopMicroservices";
        var body = $"""
                    Hi {evt.FirstName} {evt.LastName},

                    Thank you for your order!

                    Order Summary
                    ─────────────────────────────
                    Customer  : {evt.UserName}
                    Total     : {evt.TotalPrice:C}
                    Ship to   : {evt.AddressLine}, {evt.State} {evt.ZipCode}, {evt.Country}
                    Payment   : **** **** **** {(evt.CardNumber is { Length: >= 4 } c ? c[^4..] : "****")}

                    We'll notify you once your order ships.

                    Regards,
                    The Shop Team
                    """;

        await SimulateEmailAsync(evt.EmailAddress, subject, body);

        logger.LogInformation("✅ [ORDER CONFIRMATION] Dispatched to {Email}", evt.EmailAddress);

        var smsMessage = $"Hi {evt.FirstName}, your order for {evt.TotalPrice:C} has been placed!";
        var smsStatus = "Queued";

        try
        {
            await SimulateSmsAsync(evt.UserName, smsMessage);
            smsStatus = "Sent";
            logger.LogInformation("📱 [SMS] Dispatched order SMS for customer {UserName}", evt.UserName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "⚠️ SMS dispatch failed for customer {UserName}, saving audit log as Queued", evt.UserName);
        }

        // Persist email + SMS logs — idempotent upserts on EventId+Channel.
        // try/catch prevents DB failures from re-triggering delivery on MassTransit retry.
        if (repo is not null)
        {
            try
            {
                await repo.LogNotificationAsync(new NotificationLog
                {
                    EventId   = evt.Id,
                    EventType = "CartCheckout",
                    Recipient = evt.EmailAddress,
                    Channel   = "Email",
                    Subject   = subject,
                    Message   = body,
                    Status    = "Sent",
                    Metadata  = new Dictionary<string, string>
                    {
                        { "UserName",   evt.UserName },
                        { "TotalPrice", evt.TotalPrice.ToString("F2") },
                        { "Country",    evt.Country }
                    }
                });

                await repo.LogNotificationAsync(new NotificationLog
                {
                    EventId   = evt.Id,
                    EventType = "CartCheckout",
                    Recipient = evt.UserName,
                    Channel   = "SMS",
                    Subject   = "Order Confirmation SMS",
                    Message   = smsMessage,
                    Status    = smsStatus,
                    Metadata  = new Dictionary<string, string>
                    {
                        { "AddressLine", evt.AddressLine }
                    }
                });
            }
            catch (MongoDB.Driver.MongoException ex)
            {
                logger.LogWarning(ex, "⚠️ MongoDB failure persisting notification log for event {EventId}", evt.Id);
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TimeoutException ex)
            {
                logger.LogWarning(ex, "⚠️ Timed out while persisting notification log for event {EventId}", evt.Id);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "⚠️ Invalid operation persisting notification log for event {EventId}", evt.Id);
            }
        }
    }

    private static async Task SimulateEmailAsync(string to, string subject, string body)
    {
        await Task.Delay(50);
    }

    private static async Task SimulateSmsAsync(string recipient, string message)
    {
        await Task.Delay(50);
    }
}
