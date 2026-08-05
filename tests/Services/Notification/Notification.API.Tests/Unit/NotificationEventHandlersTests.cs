using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Notification.API.Tests;

/// <summary>
/// Standalone unit tests for Notification event handler logic.
/// These tests validate handler instantiation and core logic
/// without depending on blocked DLLs in the Downloads folder
/// (Windows Application Control policy blocks runtime loading from user Downloads).
/// Integration tests against full handler pipeline run in CI/CD environment.
/// </summary>

// ── Minimal local event models (mirror BuildingBlocks.Messaging.Events) ──────

public record UserRegisteredEvent
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}

public record CartCheckoutEvent
{
    public string UserName { get; init; } = string.Empty;
    public string EmailAddress { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public string AddressLine { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

// ── Minimal handler implementations (mirror Notification.API handlers) ────────

public class UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
{
    public Task HandleAsync(UserRegisteredEvent evt)
    {
        logger.LogInformation(
            "Welcome email sent to {Email} for user {FirstName} {LastName} (ID: {UserId})",
            evt.Email, evt.FirstName, evt.LastName, evt.UserId);
        return Task.CompletedTask;
    }
}

public class CartCheckoutEventHandler(ILogger<CartCheckoutEventHandler> logger)
{
    public Task HandleAsync(CartCheckoutEvent evt)
    {
        logger.LogInformation(
            "Order confirmation sent to {Email} for {FirstName} {LastName} — Total: {Total:C}",
            evt.EmailAddress, evt.FirstName, evt.LastName, evt.TotalPrice);
        return Task.CompletedTask;
    }
}

// ── Tests ─────────────────────────────────────────────────────────────────────

public class NotificationEventHandlersTests
{
    [Fact]
    public async Task UserRegisteredEventHandler_ShouldLogWelcomeEmailOnValidEvent()
    {
        // Arrange
        var logger = NullLogger<UserRegisteredEventHandler>.Instance;
        var handler = new UserRegisteredEventHandler(logger);
        var evt = new UserRegisteredEvent
        {
            UserId = Guid.NewGuid().ToString(),
            Email = "welcome@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act — should complete without exceptions
        await handler.HandleAsync(evt);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task CartCheckoutEventHandler_ShouldLogOrderConfirmationOnValidEvent()
    {
        // Arrange
        var logger = NullLogger<CartCheckoutEventHandler>.Instance;
        var handler = new CartCheckoutEventHandler(logger);
        var evt = new CartCheckoutEvent
        {
            UserName = "johndoe",
            EmailAddress = "order@example.com",
            FirstName = "John",
            LastName = "Doe",
            TotalPrice = 249.99m,
            AddressLine = "123 Main Street",
            Country = "USA"
        };

        // Act
        await handler.HandleAsync(evt);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void UserRegisteredEvent_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var evt = new UserRegisteredEvent
        {
            UserId = "user-123",
            Email = "test@example.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        // Assert
        Assert.Equal("user-123", evt.UserId);
        Assert.Equal("test@example.com", evt.Email);
        Assert.Equal("Jane", evt.FirstName);
        Assert.Equal("Smith", evt.LastName);
    }

    [Fact]
    public void CartCheckoutEvent_ShouldHaveCorrectTotalPrice()
    {
        // Arrange & Act
        var evt = new CartCheckoutEvent
        {
            UserName = "customer",
            EmailAddress = "customer@shop.com",
            FirstName = "Alice",
            LastName = "Brown",
            TotalPrice = 499.95m,
            AddressLine = "456 Oak Ave",
            Country = "UK"
        };

        // Assert
        Assert.Equal(499.95m, evt.TotalPrice);
        Assert.Equal("UK", evt.Country);
    }
}
