using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Maliev.FacilityService.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IEquipmentRepository"/>.
/// </summary>
public class EquipmentRepository : IEquipmentRepository
{
    private readonly FacilityDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="EquipmentRepository"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public EquipmentRepository(FacilityDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Equipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Equipments
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Equipment> Items, int TotalCount)> GetAllAsync(
        EquipmentFilter? filters = null,
        Pagination? pagination = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Equipments.AsQueryable();

        if (filters != null)
        {
            if (filters.Category.HasValue)
            {
                query = query.Where(e => e.Category == filters.Category.Value);
            }

            if (filters.Status.HasValue)
            {
                query = query.Where(e => e.Status == filters.Status.Value);
            }

            if (!string.IsNullOrEmpty(filters.NameContains))
            {
                query = query.Where(e => EF.Functions.ILike(e.Name, $"%{filters.NameContains}%"));
            }

            if (!string.IsNullOrEmpty(filters.Brand))
            {
                query = query.Where(e => e.Brand == filters.Brand);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (pagination != null)
        {
            query = query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize);
        }

        var items = await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Equipment>> GetActiveByCategoryAsync(
        EquipmentCategory category,
        CancellationToken cancellationToken = default)
    {
        return await _context.Equipments
            .AsNoTracking()
            .Where(e => e.Category == category && e.Status == EquipmentStatus.Active)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Equipment>> GetActiveByMultipleCategoriesAsync(
        IReadOnlySet<EquipmentCategory> categories,
        CancellationToken cancellationToken = default)
    {
        return await _context.Equipments
            .AsNoTracking()
            .Where(e => categories.Contains(e.Category) && e.Status == EquipmentStatus.Active)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Equipments
            .AsNoTracking()
            .AnyAsync(e => e.Name == name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Equipment> AddAsync(Equipment entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        
        await _context.Equipments.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return entity;
    }

    /// <inheritdoc />
    public async Task<Equipment> UpdateAsync(Equipment entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        
        _context.Equipments.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        
        return entity;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Equipments.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _context.Equipments.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
