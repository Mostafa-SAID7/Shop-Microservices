using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Notification.API.EventHandlers;

// ── User Registered → Welcome Email ──────────────────────────────────────────

public class UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
    : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation(
            "📧 [WELCOME EMAIL] Sending to {Email} — Welcome, {FirstName} {LastName}! (UserId: {UserId})",
            evt.Email, evt.FirstName, evt.LastName, evt.UserId);

        // Simulate async email dispatch
        await SimulateEmailAsync(
            to:      evt.Email,
            subject: $"Welcome to ShopMicroservices, {evt.FirstName}!",
            body:    $"""
                      Hi {evt.FirstName} {evt.LastName},

                      Your account has been created successfully.
                      User ID : {evt.UserId}

                      Start shopping now at https://shop.example.com

                      Regards,
                      The Shop Team
                      """);

        logger.LogInformation("✅ [WELCOME EMAIL] Successfully dispatched to {Email}", evt.Email);
    }

    private static async Task SimulateEmailAsync(string to, string subject, string body)
    {
        // Replace with real SMTP / SendGrid / Mailgun integration
        await Task.Delay(50);
    }
}

// ── Cart Checkout → Order Confirmation Email ──────────────────────────────────

public class CartCheckoutEventHandler(ILogger<CartCheckoutEventHandler> logger)
    : IConsumer<CartCheckoutEvent>
{
    public async Task Consume(ConsumeContext<CartCheckoutEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation(
            "🛒 [ORDER CONFIRMATION] Sending to {Email} — Order for {UserName}, Total: {Total:C}",
            evt.EmailAddress, evt.UserName, evt.TotalPrice);

        await SimulateEmailAsync(
            to:      evt.EmailAddress,
            subject: "Order Confirmation — ShopMicroservices",
            body:    $"""
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
                      """);

        logger.LogInformation("✅ [ORDER CONFIRMATION] Dispatched to {Email}", evt.EmailAddress);

        // Publish SMS notification simulation
        logger.LogInformation(
            "📱 [SMS] Sending order SMS to customer {UserName} at {AddressLine}",
            evt.UserName, evt.AddressLine);
    }

    private static async Task SimulateEmailAsync(string to, string subject, string body)
    {
        await Task.Delay(50);
    }
}

// ── Order Placed → Shipped Notification (for future Ordering.API) ─────────────

public class OrderPlacedEventHandler(ILogger<OrderPlacedEventHandler> logger)
    : IConsumer<CartCheckoutEvent>  // reuse same event shape for now
{
    public Task Consume(ConsumeContext<CartCheckoutEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation(
            "📦 [ORDER PLACED] Internal log — Customer: {UserName}, Amount: {Total:C}",
            evt.UserName, evt.TotalPrice);

        return Task.CompletedTask;
    }
}
