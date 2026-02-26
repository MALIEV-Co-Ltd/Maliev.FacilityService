using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maliev.FacilityService.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IMaintenanceLogRepository"/>.
/// </summary>
public class MaintenanceLogRepository : IMaintenanceLogRepository
{
    private readonly FacilityDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="MaintenanceLogRepository"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public MaintenanceLogRepository(FacilityDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EquipmentMaintenanceLog>> GetByEquipmentIdAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.EquipmentMaintenanceLogs
            .AsNoTracking()
            .Where(m => m.EquipmentId == equipmentId)
            .OrderByDescending(m => m.OccurredAt)
            .Take(200)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<EquipmentMaintenanceLog> AddAsync(
        EquipmentMaintenanceLog entity,
        CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        
        await _context.EquipmentMaintenanceLogs.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return entity;
    }
}
