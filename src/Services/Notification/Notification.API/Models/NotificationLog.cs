using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Notification.API.Models;

/// <summary>
/// MongoDB Document Model for notification logs persisted in notificationdb database -> NotificationLogs collection.
/// Tracks every email and SMS dispatched by the event consumers.
/// </summary>
public class NotificationLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>Idempotency key — sourced from IntegrationEvent.Id so retries are no-ops.</summary>
    [BsonElement("eventId")]
    public Guid EventId { get; set; } = Guid.Empty;

    [BsonElement("eventType")]
    public string EventType { get; set; } = string.Empty; // "UserRegistered" | "CartCheckout"

    [BsonElement("recipient")]
    public string Recipient { get; set; } = string.Empty;

    [BsonElement("channel")]
    public string Channel { get; set; } = "Email"; // "Email" | "SMS"

    [BsonElement("subject")]
    public string Subject { get; set; } = string.Empty;

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "Sent"; // "Sent" | "Failed" | "Queued"

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new();
}
