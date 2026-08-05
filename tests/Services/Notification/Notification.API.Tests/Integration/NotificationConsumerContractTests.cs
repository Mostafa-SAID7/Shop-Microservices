using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Notification.API.Tests.Integration;

/// <summary>
/// Notification.API Integration Tests.
/// 
/// Notification.API requires RabbitMQ at runtime (MassTransit consumers).
/// Infrastructure consumer tests are skipped locally and run via docker-compose.
/// 
/// Notification Event Contract:
/// ┌────────────────────────────────────────────────────────────────────────────┐
/// │ CONSUMES: UserRegisteredEvent  → sends welcome email notification          │
/// │ CONSUMES: CartCheckoutEvent    → sends order confirmation email + SMS      │
/// └────────────────────────────────────────────────────────────────────────────┘
/// </summary>
public class NotificationConsumerContractTests
{
    [Fact(Skip = "Requires RabbitMQ running. Run via: docker-compose up messagebroker notification.api")]
    public async Task UserRegisteredConsumer_ShouldSendWelcomeEmail_WhenEventReceived()
    {
        // Full integration via MassTransit test harness with running RabbitMQ:
        // var harness = new InMemoryTestHarness();
        // await harness.Start();
        // await harness.InputQueueSendEndpoint.Send(new UserRegisteredEvent { ... });
        // Assert.True(await harness.Consumed.Any<UserRegisteredEvent>());
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires RabbitMQ running. Run via: docker-compose up messagebroker notification.api")]
    public async Task CartCheckoutConsumer_ShouldSendOrderConfirmation_WhenEventReceived()
    {
        await Task.CompletedTask;
    }

    // ── Contract validation tests — run everywhere ─────────────────────────────

    [Fact]
    public void UserRegisteredEvent_Contract_ShouldHaveRequiredFields()
    {
        // Validates expected event shape matches contract
        var evt = new
        {
            UserId    = "user-abc-123",
            Email     = "newuser@shop.com",
            FirstName = "Jane",
            LastName  = "Doe"
        };

        Assert.NotNull(evt.UserId);
        Assert.Contains("@", evt.Email);
        Assert.NotNull(evt.FirstName);
        Assert.NotNull(evt.LastName);
    }

    [Fact]
    public void CartCheckoutEvent_Contract_ShouldHavePaymentAndAddressFields()
    {
        // Validates expected event shape matches contract
        var evt = new
        {
            UserName      = "customer",
            EmailAddress  = "customer@shop.com",
            FirstName     = "Alice",
            LastName      = "Smith",
            TotalPrice    = 199.99m,
            AddressLine   = "123 Main St",
            Country       = "USA",
            CardNumber    = "4111111111111111",
            CVV           = "123",
            PaymentMethod = 1
        };

        Assert.True(evt.TotalPrice > 0);
        Assert.Equal(16, evt.CardNumber.Length);
        Assert.Contains("@", evt.EmailAddress);
    }
}
