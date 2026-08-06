using Carter;
using BuildingBlocks.Messaging.Events;
using Identity.API.Models;
using Identity.API.Services;
using MassTransit;

namespace Identity.API.Endpoints;

/// <summary>
/// All Identity API endpoints:
///   POST   /api/identity/register
///   POST   /api/identity/login
///   GET    /api/identity/users/{id}
///   PUT    /api/identity/users/{id}/profile
///   PUT    /api/identity/users/{id}/change-password
///   GET    /api/identity/users           (admin)
///   PUT    /api/identity/users/{id}/role (admin)
///   GET    /api/identity/health
/// </summary>
public class IdentityEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identity").WithTags("Identity");

        // ── Auth ──────────────────────────────────────────────────────────

        group.MapPost("/register", RegisterUserAsync)
            .WithName("RegisterUser")
            .WithSummary("Register a new user account")
            .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/login", LoginAsync)
            .WithName("LoginUser")
            .WithSummary("Authenticate and receive a JWT token")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        // ── Profile ───────────────────────────────────────────────────────

        group.MapGet("/users/{id:guid}", GetProfileAsync)
            .WithName("GetUserProfile")
            .WithSummary("Get user profile by ID")
            .Produces<UserProfileDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/users/{id:guid}/profile", UpdateProfileAsync)
            .WithName("UpdateUserProfile")
            .WithSummary("Update user profile (name, username)")
            .Produces<UserProfileDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/users/{id:guid}/change-password", ChangePasswordAsync)
            .WithName("ChangePassword")
            .WithSummary("Change the user's password")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ── Admin ─────────────────────────────────────────────────────────

        group.MapGet("/users", GetAllUsersAsync)
            .WithName("GetAllUsers")
            .WithSummary("List all registered users (Admin)")
            .Produces<IReadOnlyList<UserProfileDto>>(StatusCodes.Status200OK);

        group.MapPut("/users/{id:guid}/role", ChangeRoleAsync)
            .WithName("ChangeUserRole")
            .WithSummary("Change a user's role (Admin)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ── Health ────────────────────────────────────────────────────────

        group.MapGet("/health", () => Results.Ok(new
        {
            Status = "Healthy",
            Service = "Identity.API",
            Timestamp = DateTime.UtcNow
        }))
        .WithName("IdentityHealth")
        .WithSummary("Health check endpoint");
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private static async Task<IResult> RegisterUserAsync(
        RegisterUserRequest request,
        IUserStore userStore,
        ITokenService tokenService,
        IPublishEndpoint publishEndpoint,
        ILogger<IdentityEndpoints> logger)
    {
        if (await userStore.EmailExistsAsync(request.Email))
            return Results.Conflict(new { Error = "A user with this email already exists." });

        var newUser = new User
        {
            UserName     = request.UserName,
            Email        = request.Email,
            PasswordHash = tokenService.HashPassword(request.Password),
            FirstName    = request.FirstName,
            LastName     = request.LastName,
            Role         = "Customer"
        };

        await userStore.AddAsync(newUser);

        // Publish integration event → Notification.API sends welcome email
        await publishEndpoint.Publish(new UserRegisteredEvent
        {
            UserId    = newUser.Id.ToString(),
            Email     = newUser.Email,
            FirstName = newUser.FirstName,
            LastName  = newUser.LastName
        });

        logger.LogInformation("✅ User registered: {Email} (Id={Id})", newUser.Email, newUser.Id);

        var response = new RegisterUserResponse(
            newUser.Id, newUser.UserName, newUser.Email,
            newUser.FirstName, newUser.LastName, newUser.Role, newUser.CreatedAt);

        return Results.Created($"/api/identity/users/{newUser.Id}", response);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IUserStore userStore,
        ITokenService tokenService,
        ILogger<IdentityEndpoints> logger)
    {
        var user = await userStore.FindByEmailAsync(request.Email);

        if (user is null || !tokenService.VerifyPassword(request.Password, user.PasswordHash))
        {
            logger.LogWarning("❌ Failed login attempt for email: {Email}", request.Email);
            return Results.Unauthorized();
        }

        if (!user.IsActive)
            return Results.BadRequest(new { Error = "Account is deactivated." });

        // Update last login timestamp
        user.LastLoginAt = DateTime.UtcNow;
        await userStore.UpdateAsync(user);

        var token = tokenService.GenerateJwtToken(user.Id.ToString(), user.Email, user.Role);

        logger.LogInformation("🔑 User logged in: {Email}", user.Email);

        return Results.Ok(new LoginResponse(
            token,
            "Bearer",
            TokenService.TokenExpirySeconds,
            ToProfileDto(user)));
    }

    private static async Task<IResult> GetProfileAsync(
        Guid id,
        IUserStore userStore)
    {
        var user = await userStore.FindByIdAsync(id);
        return user is null
            ? Results.NotFound(new { Error = $"User '{id}' not found." })
            : Results.Ok(ToProfileDto(user));
    }

    private static async Task<IResult> UpdateProfileAsync(
        Guid id,
        UpdateProfileRequest request,
        IUserStore userStore,
        ILogger<IdentityEndpoints> logger)
    {
        var user = await userStore.FindByIdAsync(id);
        if (user is null)
            return Results.NotFound(new { Error = $"User '{id}' not found." });

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName  is not null) user.LastName  = request.LastName;
        if (request.UserName  is not null) user.UserName  = request.UserName;

        await userStore.UpdateAsync(user);
        logger.LogInformation("✏️ Profile updated for user: {Id}", id);

        return Results.Ok(ToProfileDto(user));
    }

    private static async Task<IResult> ChangePasswordAsync(
        Guid id,
        ChangePasswordRequest request,
        IUserStore userStore,
        ITokenService tokenService,
        ILogger<IdentityEndpoints> logger)
    {
        var user = await userStore.FindByIdAsync(id);
        if (user is null)
            return Results.NotFound(new { Error = $"User '{id}' not found." });

        if (!tokenService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            return Results.BadRequest(new { Error = "Current password is incorrect." });

        user.PasswordHash = tokenService.HashPassword(request.NewPassword);
        await userStore.UpdateAsync(user);

        logger.LogInformation("🔒 Password changed for user: {Id}", id);
        return Results.NoContent();
    }

    private static async Task<IResult> GetAllUsersAsync(IUserStore userStore)
    {
        var users = await userStore.GetAllAsync();
        return Results.Ok(users.Select(ToProfileDto).ToList());
    }

    private static async Task<IResult> ChangeRoleAsync(
        Guid id,
        ChangeUserRoleRequest request,
        IUserStore userStore,
        ILogger<IdentityEndpoints> logger)
    {
        var user = await userStore.FindByIdAsync(id);
        if (user is null)
            return Results.NotFound(new { Error = $"User '{id}' not found." });

        var allowedRoles = new[] { "Customer", "Admin", "Manager" };
        if (!allowedRoles.Contains(request.Role))
            return Results.BadRequest(new { Error = $"Invalid role. Allowed: {string.Join(", ", allowedRoles)}" });

        user.Role = request.Role;
        await userStore.UpdateAsync(user);

        logger.LogInformation("👑 Role changed to '{Role}' for user: {Id}", request.Role, id);
        return Results.NoContent();
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static UserProfileDto ToProfileDto(User u) => new(
        u.Id, u.UserName, u.Email,
        u.FirstName, u.LastName, u.Role,
        u.CreatedAt, u.LastLoginAt);
}
