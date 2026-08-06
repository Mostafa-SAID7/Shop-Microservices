using Notification.API.Models;
using MongoDB.Driver;

namespace Notification.API.Services;

public interface INotificationRepository
{
    Task LogNotificationAsync(NotificationLog log);
    Task<IEnumerable<NotificationLog>> GetRecentNotificationsAsync(int limit = 50);
}

/// <summary>
/// MongoDB Repository for notification logs (notificationdb -> NotificationLogs collection).
/// Stores full audit trail of sent notifications.
/// LogNotificationAsync is idempotent: upserts on (EventId, Channel) so MassTransit
/// retries never insert duplicate audit rows.
/// </summary>
public class MongoNotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<NotificationLog> _collection;

    public MongoNotificationRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<NotificationLog>("NotificationLogs");

        // Descending timestamp index for efficient history queries
        _collection.Indexes.CreateOne(new CreateIndexModel<NotificationLog>(
            Builders<NotificationLog>.IndexKeys.Descending(n => n.Timestamp)));

        // Unique compound index on (eventId, channel) — enforces idempotency at DB level
        _collection.Indexes.CreateOne(new CreateIndexModel<NotificationLog>(
            Builders<NotificationLog>.IndexKeys
                .Ascending(n => n.EventId)
                .Ascending(n => n.Channel),
            new CreateIndexOptions { Unique = true, Sparse = true }));
    }

    /// <summary>Upserts by EventId+Channel — safe to call multiple times for the same event.</summary>
    public async Task LogNotificationAsync(NotificationLog log)
    {
        log.Timestamp = DateTime.UtcNow;

        if (log.EventId == Guid.Empty)
        {
            // No idempotency key available — plain insert (legacy path)
            await _collection.InsertOneAsync(log);
            return;
        }

        var filter = Builders<NotificationLog>.Filter.And(
            Builders<NotificationLog>.Filter.Eq(n => n.EventId, log.EventId),
            Builders<NotificationLog>.Filter.Eq(n => n.Channel,  log.Channel));

        await _collection.ReplaceOneAsync(
            filter, log,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<IEnumerable<NotificationLog>> GetRecentNotificationsAsync(int limit = 50)
    {
        return await _collection.Find(_ => true)
            .SortByDescending(n => n.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }
}
