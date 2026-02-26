using Testcontainers.PostgreSql;
using Xunit;

namespace Maliev.FacilityService.Tests;

#pragma warning disable CS0618 // Type or member is obsolete
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgresFixture>
{
}

[CollectionDefinition("PostgresCollection")]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
}

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("maliev_facility_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
#pragma warning restore CS0618
