using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.FacilityService.Infrastructure.Data;

public class FacilityDbContextFactory : IDesignTimeDbContextFactory<FacilityDbContext>
{
    public FacilityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FacilityDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("EF_CONNECTION_STRING")
            ?? "Host=localhost;Database=facility-app-db;Username=postgres;Password=__PLACEHOLDER__";
        optionsBuilder.UseNpgsql(connectionString);

        return new FacilityDbContext(optionsBuilder.Options);
    }
}
