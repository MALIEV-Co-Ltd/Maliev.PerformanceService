using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Maliev.PerformanceService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MassTransit;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Testcontainers.RabbitMq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Integration;

/// <summary>
/// Base class for integration tests providing a configured WebApplicationFactory and PostgreSQL container.
/// </summary>
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    /// <summary>
    /// PostgreSQL container for integration testing.
    /// </summary>
    protected readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:18-alpine")
        .Build();

    /// <summary>
    /// Redis container for integration testing.
    /// </summary>
    protected readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:8.4-alpine")
        .Build();

    /// <summary>
    /// RabbitMQ container for integration testing.
    /// </summary>
    protected readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:4.2-alpine")
        .Build();

    /// <summary>
    /// HTTP client for making requests to the test server.
    /// </summary>
    protected HttpClient _client = null!;

    /// <summary>
    /// Web application factory for the performance service.
    /// </summary>
    protected WebApplicationFactory<Program> _factory = null!;

    /// <summary>
    /// Current user identifier for authenticated requests.
    /// </summary>
    protected Guid _currentUserId = Guid.NewGuid();

    /// <inheritdoc/>
    public virtual async Task InitializeAsync()
    {
        await Task.WhenAll(
            _dbContainer.StartAsync(),
            _redisContainer.StartAsync(),
            _rabbitMqContainer.StartAsync()
        );

        // Set environment variables for connection strings (read early in configuration pipeline)
        Environment.SetEnvironmentVariable("ConnectionStrings__redis", _redisContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", _rabbitMqContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__PerformanceDbContext", _dbContainer.GetConnectionString());

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureTestServices(services =>
                {
                    // Configure JWT Bearer authentication with test RSA key
                    services.PostConfigureAll<JwtBearerOptions>(options =>
                    {
                        options.MapInboundClaims = false;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = false,
                            ValidateIssuerSigningKey = false,
                            SignatureValidator = (token, parameters) => new JwtSecurityToken(token)
                        };
                    });

                    // Add MassTransit Test Harness (overrides standard registration)
                    services.AddMassTransitTestHarness();

                    // Add Mock Authentication
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                    }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                });
            });

        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        // Migrate DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PerformanceDbContext>();
        await db.Database.MigrateAsync();
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _dbContainer.StopAsync(),
            _redisContainer.StopAsync(),
            _rabbitMqContainer.StopAsync()
        );
        _factory.Dispose();
    }

    /// <summary>
    /// Mock authentication handler for integration tests.
    /// </summary>
    protected class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /// <summary>
        /// Gets or sets the user identifier to use for the authenticated principal.
        /// </summary>
        public static Guid UserId = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAuthHandler"/> class.
        /// </summary>
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder)
            : base(options, logger, encoder) { }

        /// <inheritdoc/>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, UserId.ToString()) };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}