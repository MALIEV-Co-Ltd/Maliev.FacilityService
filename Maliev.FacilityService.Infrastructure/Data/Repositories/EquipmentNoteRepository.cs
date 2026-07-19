using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maliev.FacilityService.Infrastructure.Data.Repositories;

/// <summary>
/// PostgreSQL implementation of the equipment note repository.
/// </summary>
public class EquipmentNoteRepository : IEquipmentNoteRepository
{
    private readonly FacilityDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of <see cref="EquipmentNoteRepository"/>.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public EquipmentNoteRepository(FacilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<EquipmentNote> AddAsync(EquipmentNote note, CancellationToken cancellationToken = default)
    {
        var entry = await _dbContext.EquipmentNotes.AddAsync(note, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EquipmentNote>> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EquipmentNotes
            .AsNoTracking()
            .Where(n => n.EquipmentId == equipmentId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);
    }
}
