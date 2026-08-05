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
/// </summary>
public class MongoNotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<NotificationLog> _collection;

    public MongoNotificationRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<NotificationLog>("NotificationLogs");

        // Index on timestamp descending for efficient history queries
        var indexKeys = Builders<NotificationLog>.IndexKeys.Descending(n => n.Timestamp);
        _collection.Indexes.CreateOne(new CreateIndexModel<NotificationLog>(indexKeys));
    }

    public async Task LogNotificationAsync(NotificationLog log)
    {
        log.Timestamp = DateTime.UtcNow;
        await _collection.InsertOneAsync(log);
    }

    public async Task<IEnumerable<NotificationLog>> GetRecentNotificationsAsync(int limit = 50)
    {
        return await _collection.Find(_ => true)
            .SortByDescending(n => n.Timestamp)
            .Limit(limit)
            .ToListAsync();
    }
}
