using Identity.API.Models;

namespace Identity.API.Services;

/// <summary>
/// Thread-safe in-memory user store. Replace with EF Core + PostgreSQL for production.
/// </summary>
public interface IUserStore
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(Guid id);
    Task<User?> FindByUserNameAsync(string userName);
    Task<IReadOnlyList<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UserNameExistsAsync(string userName);
}

public class InMemoryUserStore : IUserStore
{
    private readonly List<User> _users = [];
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<User?> FindByEmailAsync(string email)
    {
        await _lock.WaitAsync();
        try { return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)); }
        finally { _lock.Release(); }
    }

    public async Task<User?> FindByIdAsync(Guid id)
    {
        await _lock.WaitAsync();
        try { return _users.FirstOrDefault(u => u.Id == id); }
        finally { _lock.Release(); }
    }

    public async Task<User?> FindByUserNameAsync(string userName)
    {
        await _lock.WaitAsync();
        try { return _users.FirstOrDefault(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)); }
        finally { _lock.Release(); }
    }

    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try { return _users.ToList().AsReadOnly(); }
        finally { _lock.Release(); }
    }

    public async Task AddAsync(User user)
    {
        await _lock.WaitAsync();
        try { _users.Add(user); }
        finally { _lock.Release(); }
    }

    public async Task UpdateAsync(User user)
    {
        await _lock.WaitAsync();
        try
        {
            var idx = _users.FindIndex(u => u.Id == user.Id);
            if (idx >= 0) _users[idx] = user;
        }
        finally { _lock.Release(); }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        await _lock.WaitAsync();
        try { return _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)); }
        finally { _lock.Release(); }
    }

    public async Task<bool> UserNameExistsAsync(string userName)
    {
        await _lock.WaitAsync();
        try { return _users.Any(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)); }
        finally { _lock.Release(); }
    }
}
