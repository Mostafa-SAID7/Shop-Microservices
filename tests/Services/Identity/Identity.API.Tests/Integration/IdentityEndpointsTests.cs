using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Identity.API.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory — replaces RabbitMQ MassTransit transport
/// with the in-memory bus so integration tests run without any broker.
/// </summary>
public sealed class IdentityTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        // Inject a test-only JWT secret so the host starts without any real secrets.
        // This is intentionally isolated from production config — never a shared fallback.
        builder.ConfigureAppConfiguration(cfg =>
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = "TestOnlySecret_NotUsedInProduction_32chars!"
            }));

        builder.ConfigureServices(services =>
        {
            // Remove ALL MassTransit registrations (bus + hosted services).
            // Only removing IBus is not enough — the RabbitMQ IHostedService entries
            // still try to connect to the broker at WebApplicationFactory startup,
            // causing the test host to crash before any test runs.
            var massTransitDescriptors = services
                .Where(d =>
                    d.ServiceType.FullName != null &&
                    d.ServiceType.FullName.StartsWith("MassTransit", StringComparison.Ordinal))
                .ToList();
            foreach (var d in massTransitDescriptors)
                services.Remove(d);

            // Also remove the generic IHostedService entries that MassTransit registers
            // (IBusControl, etc.) so nothing tries to dial RabbitMQ at startup.
            var hostedServices = services
                .Where(d =>
                    d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
                    d.ImplementationType?.FullName != null &&
                    d.ImplementationType.FullName.Contains("MassTransit",
                        StringComparison.Ordinal))
                .ToList();
            foreach (var d in hostedServices)
                services.Remove(d);

            // Re-register MassTransit with in-memory transport only — no broker needed.
            services.AddMassTransit(x =>
            {
                x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
            });
        });
    }
}

/// <summary>
/// Integration tests for Identity.API HTTP endpoints.
/// Uses <see cref="IdentityTestFactory"/> which swaps RabbitMQ → in-memory.
/// Runs fully locally — no Docker, no broker required.
/// </summary>
public class IdentityEndpointsTests : IClassFixture<IdentityTestFactory>
{
    private readonly HttpClient _client;

    public IdentityEndpointsTests(IdentityTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GET_Health_ShouldReturn200()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task POST_Register_ShouldReturn201_WhenValidRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/identity/register", new
        {
            UserName  = $"user_{Guid.NewGuid():N}",
            Email     = $"{Guid.NewGuid():N}@example.com",
            Password  = "SecurePass@2026",
            FirstName = "Test",
            LastName  = "User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await response.Content.ReadAsStringAsync()).Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task POST_Login_ShouldReturn200_WithToken_WhenValidCredentials()
    {
        var email    = $"{Guid.NewGuid():N}@example.com";
        var password = "LoginPass@2026";

        await _client.PostAsJsonAsync("/api/identity/register", new
        {
            UserName  = $"user_{Guid.NewGuid():N}",
            Email     = email,
            Password  = password,
            FirstName = "Login", LastName = "Test"
        });

        var response = await _client.PostAsJsonAsync("/api/identity/login",
            new { Email = email, Password = password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("token");
    }

    [Fact]
    public async Task POST_Login_ShouldReturn401_WhenWrongCredentials()
    {
        var response = await _client.PostAsJsonAsync("/api/identity/login",
            new { Email = "nobody@example.com", Password = "BadPass!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_Register_ShouldReturn409_WhenDuplicateEmail()
    {
        var email = $"dup_{Guid.NewGuid():N}@example.com";

        await _client.PostAsJsonAsync("/api/identity/register", new
        {
            UserName  = $"user_{Guid.NewGuid():N}",
            Email     = email,
            Password  = "Pass@2026",
            FirstName = "A", LastName = "B"
        });

        var response = await _client.PostAsJsonAsync("/api/identity/register", new
        {
            UserName  = $"user2_{Guid.NewGuid():N}",
            Email     = email,         // same email — should conflict
            Password  = "Pass@2026",
            FirstName = "C", LastName = "D"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
