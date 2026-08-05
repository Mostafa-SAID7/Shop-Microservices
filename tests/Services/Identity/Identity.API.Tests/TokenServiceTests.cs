using Identity.API.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Identity.API.Tests;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"JwtSettings:Secret", "SuperSecretKeyForTestingShopMicroservicesIdentityApi_2026!"},
            {"JwtSettings:Issuer", "ShopMicroservicesIdentityApiTest"},
            {"JwtSettings:Audience", "ShopMicroservicesClientsTest"}
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _tokenService = new TokenService(config);
    }

    [Fact]
    public void HashPassword_ShouldReturnSaltedBase64Hash_WhenPasswordProvided()
    {
        // Arrange
        var rawPassword = "SecureTestPassword123!";

        // Act
        var hash = _tokenService.HashPassword(rawPassword);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual(rawPassword, hash);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        // Arrange
        var rawPassword = "MySuperSecretPassword@2026";
        var hash = _tokenService.HashPassword(rawPassword);

        // Act
        var isMatch = _tokenService.VerifyPassword(rawPassword, hash);

        // Assert
        Assert.True(isMatch);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        // Arrange
        var rawPassword = "ValidPassword123";
        var wrongPassword = "WrongPassword123";
        var hash = _tokenService.HashPassword(rawPassword);

        // Act
        var isMatch = _tokenService.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(isMatch);
    }

    [Fact]
    public void GenerateJwtToken_ShouldReturnValidJwtTokenString()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var email = "testuser@example.com";
        var role = "Customer";

        // Act
        var token = _tokenService.GenerateJwtToken(userId, email, role);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // Standard JWT contains header.payload.signature
    }
}
