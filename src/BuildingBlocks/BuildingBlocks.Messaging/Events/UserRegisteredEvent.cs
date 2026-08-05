namespace BuildingBlocks.Messaging.Events;

public record UserRegisteredEvent : IntegrationEvent
{
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}
