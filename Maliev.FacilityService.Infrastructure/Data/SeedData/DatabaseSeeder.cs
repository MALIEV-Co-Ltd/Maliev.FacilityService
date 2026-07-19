using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maliev.FacilityService.Infrastructure.Data.SeedData;

/// <summary>
/// Handles initial database seeding for facility equipment data.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds manufacturing equipment if the table is empty.
    /// </summary>
    public static async Task SeedEquipmentsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FacilityDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");

        try
        {
            var equipment = EquipmentSeedData.GetAll().ToList();
            var existingIds = await context.Equipments
                .AsNoTracking()
                .Select(equipment => equipment.Id)
                .ToListAsync();
            var missingEquipment = equipment
                .Where(equipment => !existingIds.Contains(equipment.Id))
                .ToList();

            if (missingEquipment.Count == 0)
            {
                logger.LogInformation("All seeded manufacturing equipment already exists. Skipping seed.");
                return;
            }

            logger.LogInformation("Seeding manufacturing equipment...");

            var now = DateTime.UtcNow;
            foreach (var e in missingEquipment)
            {
                e.CreatedAt = now;
                e.UpdatedAt = now;
            }

            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await context.Database.BeginTransactionAsync();

                // For TPT inheritance, just add to the base DbSet - EF Core handles the derived type tables automatically
                foreach (var item in missingEquipment)
                {
                    await context.Equipments.AddAsync(item);
                }

                await context.SaveChangesAsync();
                await tx.CommitAsync();
            });

            logger.LogInformation("Seeded {Count} equipment items.", missingEquipment.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding equipment.");
        }
    }
}
