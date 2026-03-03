using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Maliev.FacilityService.Infrastructure.Data;
using MassTransit.Testing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace Maliev.FacilityService.Tests.Infrastructure;

public class FacilityServiceTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static PostgreSqlContainer? _postgresContainer;
    private static RabbitMqContainer? _rabbitMqContainer;
    private static bool _containersStarted;
    private static readonly SemaphoreSlim _initLock = new(1, 1);
    private readonly RSA _testRsa;

    public FacilityServiceTestFactory()
    {
        _testRsa = RSA.Create(2048);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        var publicKeyPem = _testRsa.ExportRSAPublicKeyPem();
        var publicKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(publicKeyPem));
        Environment.SetEnvironmentVariable("Jwt__PublicKey", publicKeyBase64);
        Environment.SetEnvironmentVariable("Jwt__SecurityKey", "test-secret-key-at-least-32-characters-long");
    }

    public async Task InitializeAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            if (!_containersStarted)
            {
                _postgresContainer = new PostgreSqlBuilder("postgres:15-alpine")
                    .WithDatabase("maliev_facility_test")
                    .WithUsername("postgres")
                    .WithPassword("postgres")
                    .Build();

                _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:3.12-alpine")
                    .Build();

                await Task.WhenAll(
                    _postgresContainer.StartAsync(),
                    _rabbitMqContainer.StartAsync()
                );

                var postgresReady = false;
                var retryCount = 0;
                const int maxRetries = 60;
                while (!postgresReady && retryCount < maxRetries)
                {
                    try
                    {
                        await using var conn = new Npgsql.NpgsqlConnection(_postgresContainer.GetConnectionString());
                        await conn.OpenAsync();
                        await using var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT 1";
                        await cmd.ExecuteScalarAsync();
                        postgresReady = true;
                    }
                    catch
                    {
                        retryCount++;
                        await Task.Delay(1000);
                    }
                }

                if (!postgresReady)
                {
                    throw new InvalidOperationException("PostgreSQL Testcontainer failed to become ready after 60 seconds.");
                }

                await ApplyMigrationsAsync();
                _containersStarted = true;
            }
        }
        finally
        {
            _initLock.Release();
        }

        Environment.SetEnvironmentVariable($"ConnectionStrings__FacilityDbContext", _postgresContainer!.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", _rabbitMqContainer!.GetConnectionString());
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        _testRsa.Dispose();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        if (!_containersStarted)
        {
            InitializeAsync().GetAwaiter().GetResult();
        }

        var rsaParams = _testRsa.ExportParameters(false);
        Environment.SetEnvironmentVariable("JWT_PUBLIC_KEY_MODULUS", Convert.ToBase64String(rsaParams.Modulus!));
        Environment.SetEnvironmentVariable("JWT_PUBLIC_KEY_EXPONENT", Convert.ToBase64String(rsaParams.Exponent!));

        var keyBytes = _testRsa.ExportSubjectPublicKeyInfo();
        Environment.SetEnvironmentVariable("Authentication__Jwt__PublicKey", Convert.ToBase64String(keyBytes));

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecurityKey"] = "test-secret-key-at-least-32-characters-long",
                ["ConnectionStrings:FacilityDbContext"] = _postgresContainer!.GetConnectionString(),
                ["ConnectionStrings:rabbitmq"] = _rabbitMqContainer!.GetConnectionString(),
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Note: AddMassTransitTestHarness skipped - tests don't require message verification
            // services.AddMassTransitTestHarness();

            var iamMock = new Mock<Maliev.Aspire.ServiceDefaults.IAM.IIamServiceClient>();
            iamMock.Setup(x => x.CheckPermissionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            iamMock.Setup(x => x.GetUserPermissionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());
            services.AddSingleton(iamMock.Object);

            var statusTracker = new Maliev.Aspire.ServiceDefaults.IAM.IAMRegistrationStatusTracker();
            statusTracker.MarkRegistered();
            services.AddSingleton(statusTracker);

            services.PostConfigureAll<JwtBearerOptions>(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "test-issuer",
                    ValidAudience = "test-audience",
                    IssuerSigningKey = new RsaSecurityKey(_testRsa),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };
            });
        });
    }

    private async Task ApplyMigrationsAsync()
    {
        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();
    }

    public FacilityDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<FacilityDbContext>();
    }

    public FacilityDbContext CreateDbContext()
    {
        var connectionString = _postgresContainer!.GetConnectionString();
        var optionsBuilder = new DbContextOptionsBuilder<FacilityDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new FacilityDbContext(optionsBuilder.Options);
    }

    public async Task CleanDatabaseAsync()
    {
        await using var context = CreateDbContext();
        var tableNames = await context.Database
            .SqlQueryRaw<string>(
                @"SELECT table_name
                  FROM information_schema.tables
                  WHERE table_schema = 'public'
                  AND table_type = 'BASE TABLE'
                  AND table_name != '__EFMigrationsHistory'
                  ORDER BY table_name")
            .ToListAsync();

        foreach (var tableName in tableNames)
        {
            try
            {
#pragma warning disable EF1002
                await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{tableName}\" RESTART IDENTITY CASCADE");
#pragma warning restore EF1002
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01")
            {
            }
        }
    }

    public string CreateTestJwtToken(
        string userId = "test-user",
        string[]? roles = null,
        string[]? permissions = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (roles != null)
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));
        if (permissions != null)
            foreach (var permission in permissions)
                claims.Add(new Claim("permissions", permission));

        var rsaSecurityKey = new RsaSecurityKey(_testRsa);
        var signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: "test-issuer",
            audience: "test-audience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public HttpClient CreateAuthenticatedClient(string userId = "test-user", string[]? roles = null, string[]? permissions = null)
    {
        var token = CreateTestJwtToken(userId, roles, permissions);
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }
}

[CollectionDefinition("FacilityApiCollection")]
public class FacilityApiCollection : ICollectionFixture<FacilityServiceTestFactory>
{
}
