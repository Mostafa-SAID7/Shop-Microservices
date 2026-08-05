namespace Identity.API.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Role { get; set; } = "Customer";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}

// ── Register ──────────────────────────────────────────────────────────────
public record RegisterUserRequest(
    string UserName,
    string Email,
    string Password,
    string FirstName,
    string LastName);

public record RegisterUserResponse(
    Guid Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime CreatedAt);

// ── Login ─────────────────────────────────────────────────────────────────
public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Token,
    string TokenType,
    int ExpiresInSeconds,
    UserProfileDto User);

// ── Profile ───────────────────────────────────────────────────────────────
public record UserProfileDto(
    Guid Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

// ── Update Profile ────────────────────────────────────────────────────────
public record UpdateProfileRequest(
    string? FirstName,
    string? LastName,
    string? UserName);

// ── Change Password ───────────────────────────────────────────────────────
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

// ── Admin ─────────────────────────────────────────────────────────────────
public record ChangeUserRoleRequest(string Role);
