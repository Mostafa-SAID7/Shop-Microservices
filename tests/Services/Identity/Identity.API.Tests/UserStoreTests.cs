using Identity.API.Models;
using Identity.API.Services;
using Xunit;

namespace Identity.API.Tests;

public class UserStoreTests
{
    private readonly InMemoryUserStore _userStore = new();

    [Fact]
    public async Task AddAsync_ShouldAddUserToStore()
    {
        // Arrange
        var user = new User
        {
            UserName = "john_doe",
            Email = "john@example.com",
            PasswordHash = "dummyhash",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        await _userStore.AddAsync(user);
        var fetchedUser = await _userStore.FindByEmailAsync("john@example.com");

        // Assert
        Assert.NotNull(fetchedUser);
        Assert.Equal("john_doe", fetchedUser.UserName);
    }

    [Fact]
    public async Task EmailExistsAsync_ShouldReturnTrue_WhenEmailExists()
    {
        // Arrange
        var user = new User
        {
            UserName = "jane_doe",
            Email = "jane@example.com",
            PasswordHash = "hash",
            FirstName = "Jane",
            LastName = "Doe"
        };
        await _userStore.AddAsync(user);

        // Act
        var exists = await _userStore.EmailExistsAsync("jane@example.com");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingUserFields()
    {
        // Arrange
        var user = new User
        {
            UserName = "alice",
            Email = "alice@example.com",
            PasswordHash = "hash",
            FirstName = "Alice",
            LastName = "Smith"
        };
        await _userStore.AddAsync(user);

        // Act
        user.FirstName = "AliceUpdated";
        await _userStore.UpdateAsync(user);

        var updatedUser = await _userStore.FindByIdAsync(user.Id);

        // Assert
        Assert.NotNull(updatedUser);
        Assert.Equal("AliceUpdated", updatedUser.FirstName);
    }
}
