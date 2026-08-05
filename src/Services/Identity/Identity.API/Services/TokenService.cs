using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API.Services;

public interface ITokenService
{
    string GenerateJwtToken(string userId, string email, string role);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

/// <summary>
/// Generates JWT access tokens and handles password hashing using BCrypt-style PBKDF2.
/// </summary>
public class TokenService(IConfiguration config) : ITokenService
{
    private const int TokenExpiryHours = 24;

    public string GenerateJwtToken(string userId, string email, string role)
    {
        var secretKey = config["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured.");
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role,               role),
            new Claim("role",                        role)  // OIDC-compatible
        };

        var token = new JwtSecurityToken(
            issuer:            config["JwtSettings:Issuer"]   ?? "ShopMicroservicesIdentityApi",
            audience:          config["JwtSettings:Audience"] ?? "ShopMicroservicesClients",
            claims:            claims,
            notBefore:         DateTime.UtcNow,
            expires:           DateTime.UtcNow.AddHours(TokenExpiryHours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Uses PBKDF2/SHA256 with a random salt embedded in the result.</summary>
    public string HashPassword(string password)
    {
        // Generate a random 16-byte salt
        var salt = new byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(salt);

        // Derive 32-byte key via PBKDF2 with 100_000 iterations
        var key = System.Security.Cryptography.Rfc2898DeriveBytes
            .Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000,
                    System.Security.Cryptography.HashAlgorithmName.SHA256, 32);

        // Store salt + key together as Base64
        var combined = new byte[salt.Length + key.Length];
        Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
        Buffer.BlockCopy(key,  0, combined, salt.Length, key.Length);
        return Convert.ToBase64String(combined);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var combined = Convert.FromBase64String(passwordHash);
        var salt     = combined[..16];
        var storedKey = combined[16..];

        var derivedKey = System.Security.Cryptography.Rfc2898DeriveBytes
            .Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000,
                    System.Security.Cryptography.HashAlgorithmName.SHA256, 32);

        return storedKey.SequenceEqual(derivedKey);
    }

    public static int TokenExpirySeconds => TokenExpiryHours * 3600;
}
