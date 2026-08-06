using Identity.API.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Identity.API.Services;

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Production MongoDB implementation of IUserStore.
/// Persists identity users into MongoDB: database = "identitydb", collection = "Users".
/// Unique index enforced on Email and UserName.
/// </summary>
public class MongoUserStore : IUserStore
{
    private readonly IMongoCollection<MongoUserDocument> _users;

    public MongoUserStore(IMongoDatabase database)
    {
        _users = database.GetCollection<MongoUserDocument>("Users");

        // Unique index on Email
        _users.Indexes.CreateOne(new CreateIndexModel<MongoUserDocument>(
            Builders<MongoUserDocument>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true, Name = "idx_email_unique" }));

        // Unique index on UserName
        _users.Indexes.CreateOne(new CreateIndexModel<MongoUserDocument>(
            Builders<MongoUserDocument>.IndexKeys.Ascending(u => u.UserName),
            new CreateIndexOptions { Unique = true, Name = "idx_username_unique" }));
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        var doc = await _users
            .Find(Builders<MongoUserDocument>.Filter.Eq(u => u.Email, email.ToLowerInvariant().Trim()))
            .FirstOrDefaultAsync();
        return doc is null ? null : MapToDomain(doc);
    }

    public async Task<User?> FindByIdAsync(Guid id)
    {
        var doc = await _users
            .Find(Builders<MongoUserDocument>.Filter.Eq(u => u.DomainId, id.ToString()))
            .FirstOrDefaultAsync();
        return doc is null ? null : MapToDomain(doc);
    }

    public async Task<User?> FindByUserNameAsync(string userName)
    {
        var doc = await _users
            .Find(Builders<MongoUserDocument>.Filter.Eq(u => u.UserName, userName))
            .FirstOrDefaultAsync();
        return doc is null ? null : MapToDomain(doc);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        var docs = await _users.Find(_ => true).ToListAsync();
        return docs.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task AddAsync(User user)
    {
        var doc = new MongoUserDocument
        {
            DomainId    = user.Id.ToString(),
            UserName    = user.UserName,
            Email       = user.Email.ToLowerInvariant().Trim(),
            PasswordHash = user.PasswordHash,
            FirstName   = user.FirstName,
            LastName    = user.LastName,
            Role        = user.Role,
            IsActive    = user.IsActive,
            CreatedAt   = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            UpdatedAt   = DateTime.UtcNow
        };

        try
        {
            await _users.InsertOneAsync(doc);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey || ex.WriteError?.Code == 11000)
        {
            throw new UserAlreadyExistsException("A user with this email or username already exists.", ex);
        }
    }

    public async Task UpdateAsync(User user)
    {
        var filter = Builders<MongoUserDocument>.Filter.Eq(u => u.DomainId, user.Id.ToString());
        var update = Builders<MongoUserDocument>.Update
            .Set(u => u.FirstName,    user.FirstName)
            .Set(u => u.LastName,     user.LastName)
            .Set(u => u.UserName,     user.UserName)
            .Set(u => u.PasswordHash, user.PasswordHash)
            .Set(u => u.Role,         user.Role)
            .Set(u => u.IsActive,     user.IsActive)
            .Set(u => u.LastLoginAt,  user.LastLoginAt)
            .Set(u => u.UpdatedAt,    DateTime.UtcNow);

        try
        {
            await _users.UpdateOneAsync(filter, update);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey || ex.WriteError?.Code == 11000)
        {
            throw new UserAlreadyExistsException("A user with this email or username already exists.", ex);
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _users
            .Find(Builders<MongoUserDocument>.Filter.Eq(u => u.Email, email.ToLowerInvariant().Trim()))
            .AnyAsync();
    }

    public async Task<bool> UserNameExistsAsync(string userName)
    {
        return await _users
            .Find(Builders<MongoUserDocument>.Filter.Eq(u => u.UserName, userName))
            .AnyAsync();
    }

    // ── Internal document model stored in MongoDB ─────────────────────────────

    private sealed class MongoUserDocument
    {
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(BsonType.ObjectId)]
        public string MongoId { get; set; } = string.Empty;

        /// <summary>Domain Guid stored as string for round-trip fidelity.</summary>
        [MongoDB.Bson.Serialization.Attributes.BsonElement("domainId")]
        public string DomainId { get; set; } = string.Empty;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("userName")]
        public string UserName { get; set; } = string.Empty;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("lastName")]
        public string LastName { get; set; } = string.Empty;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("role")]
        public string Role { get; set; } = "Customer";

        [MongoDB.Bson.Serialization.Attributes.BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MongoDB.Bson.Serialization.Attributes.BsonElement("lastLoginAt")]
        public DateTime? LastLoginAt { get; set; }

        [MongoDB.Bson.Serialization.Attributes.BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    private static User MapToDomain(MongoUserDocument doc) => new User
    {
        Id           = Guid.TryParse(doc.DomainId, out var g) ? g : Guid.Empty,
        UserName     = doc.UserName,
        Email        = doc.Email,
        PasswordHash = doc.PasswordHash,
        FirstName    = doc.FirstName,
        LastName     = doc.LastName,
        Role         = doc.Role,
        IsActive     = doc.IsActive,
        CreatedAt    = doc.CreatedAt,
        LastLoginAt  = doc.LastLoginAt
    };
}
