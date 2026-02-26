using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maliev.FacilityService.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IAttachmentRepository"/>.
/// </summary>
public class AttachmentRepository : IAttachmentRepository
{
    private readonly FacilityDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="AttachmentRepository"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public AttachmentRepository(FacilityDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EquipmentAttachment>> GetByEquipmentIdAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.EquipmentAttachments
            .AsNoTracking()
            .Where(a => a.EquipmentId == equipmentId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EquipmentAttachment>> GetActiveByEquipmentIdAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.EquipmentAttachments
            .AsNoTracking()
            .Where(a => a.EquipmentId == equipmentId && a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<EquipmentAttachment> AddAsync(
        EquipmentAttachment entity,
        CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        
        await _context.EquipmentAttachments.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return entity;
    }

    /// <inheritdoc />
    public async Task<EquipmentAttachment> UpdateAsync(
        EquipmentAttachment entity,
        CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        
        _context.EquipmentAttachments.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        
        return entity;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.EquipmentAttachments.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _context.EquipmentAttachments.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
