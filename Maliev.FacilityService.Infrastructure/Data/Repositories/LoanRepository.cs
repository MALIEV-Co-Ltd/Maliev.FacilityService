using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Maliev.FacilityService.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ILoanRepository"/>.
/// </summary>
public class LoanRepository : ILoanRepository
{
    private readonly FacilityDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="LoanRepository"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public LoanRepository(FacilityDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<EquipmentLoan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var loan = await _context.EquipmentLoans
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        return loan;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EquipmentLoan>> GetByEquipmentIdAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.EquipmentLoans
            .AsNoTracking()
            .Where(l => l.EquipmentId == equipmentId)
            .OrderByDescending(l => l.LoanStartDate)
            .Take(200)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<EquipmentLoan?> GetActiveLoanByEquipmentIdAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.EquipmentLoans
            .AsNoTracking()
            .FirstOrDefaultAsync(l =>
                l.EquipmentId == equipmentId &&
                l.LoanStatus == LoanStatus.Active,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasActiveLoanAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.EquipmentLoans
            .AsNoTracking()
            .AnyAsync(l =>
                l.EquipmentId == equipmentId &&
                (l.LoanStatus == LoanStatus.Active || l.LoanStatus == LoanStatus.Approved || l.LoanStatus == LoanStatus.Pending),
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<EquipmentLoan> AddAsync(EquipmentLoan entity, CancellationToken cancellationToken = default)
    {
        await _context.EquipmentLoans.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc />
    public async Task<EquipmentLoan> UpdateAsync(EquipmentLoan entity, CancellationToken cancellationToken = default)
    {
        _context.EquipmentLoans.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc />
    public async Task<EquipmentLoan> UpdateAsync(EquipmentLoan entity, uint rowVersion, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _context.EquipmentLoans.Update(entity);
        }
        entry.Property("xmin").OriginalValue = rowVersion;
        await _context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc />
    public void SetXminOriginalValue(EquipmentLoan entity, uint rowVersion)
    {
        _context.Entry(entity).Property("xmin").OriginalValue = rowVersion;
    }
}
