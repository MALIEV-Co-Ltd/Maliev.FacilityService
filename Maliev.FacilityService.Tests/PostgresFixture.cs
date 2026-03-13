using Testcontainers.PostgreSql;
using Xunit;

namespace Maliev.FacilityService.Tests;

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
    private readonly PostgreSqlContainer _dbContainer = 
                #pragma warning disable CS0618
        new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("maliev_facility_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
#pragma warning restore CS0618

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



