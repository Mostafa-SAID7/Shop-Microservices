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

        // Persist notification log to MongoDB (notificationdb -> NotificationLogs)
        if (repo is not null)
        {
            await repo.LogNotificationAsync(new NotificationLog
            {
                EventType = "UserRegistered",
                Recipient = evt.Email,
                Channel = "Email",
                Subject = subject,
                Message = body,
                Status = "Sent",
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", evt.UserId },
                    { "FirstName", evt.FirstName },
                    { "LastName", evt.LastName }
                }
            });
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
                    Payment   : **** **** **** {evt.CardNumber[^4..]}

                    We'll notify you once your order ships.

                    Regards,
                    The Shop Team
                    """;

        await SimulateEmailAsync(evt.EmailAddress, subject, body);

        logger.LogInformation("✅ [ORDER CONFIRMATION] Dispatched to {Email}", evt.EmailAddress);

        // Persist email notification log to MongoDB
        if (repo is not null)
        {
            await repo.LogNotificationAsync(new NotificationLog
            {
                EventType = "CartCheckout",
                Recipient = evt.EmailAddress,
                Channel = "Email",
                Subject = subject,
                Message = body,
                Status = "Sent",
                Metadata = new Dictionary<string, string>
                {
                    { "UserName", evt.UserName },
                    { "TotalPrice", evt.TotalPrice.ToString("F2") },
                    { "Country", evt.Country }
                }
            });

            // Persist SMS notification log to MongoDB
            await repo.LogNotificationAsync(new NotificationLog
            {
                EventType = "CartCheckout",
                Recipient = evt.UserName,
                Channel = "SMS",
                Subject = "Order Confirmation SMS",
                Message = $"Hi {evt.FirstName}, your order for {evt.TotalPrice:C} has been placed!",
                Status = "Sent",
                Metadata = new Dictionary<string, string>
                {
                    { "AddressLine", evt.AddressLine }
                }
            });
        }

        logger.LogInformation("📱 [SMS] Dispatched order SMS for customer {UserName}", evt.UserName);
    }

    private static async Task SimulateEmailAsync(string to, string subject, string body)
    {
        await Task.Delay(50);
    }
}
