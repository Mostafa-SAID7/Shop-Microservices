using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        builder.ConfigureServices(services =>
        {
            // Remove all MassTransit hosted/bus registrations
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IBus));
            if (descriptor is not null) services.Remove(descriptor);

            // Re-register MassTransit using in-memory transport only
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
