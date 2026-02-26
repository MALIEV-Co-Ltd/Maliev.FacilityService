using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Maliev.FacilityService.Infrastructure.Services;

/// <summary>
/// Generates unique asset codes for equipment using per-category sequences persisted in the database.
/// Asset codes follow the format: <c>MAL-{PREFIX}-{SEQ:D4}</c>.
/// </summary>
public class AssetCodeGenerator : IAssetCodeGenerator
{
    private readonly FacilityDbContext _context;

    private static readonly Dictionary<EquipmentCategory, string> CategoryPrefixes = new()
    {
        { EquipmentCategory.FdmPrinter, "FDM" },
        { EquipmentCategory.SlaPrinter, "SLA" },
        { EquipmentCategory.CncMachine, "CNC" },
        { EquipmentCategory.Scanner3D, "3DS" },
        { EquipmentCategory.InjectionMolding, "IM" },
        { EquipmentCategory.OfficeEquipment, "OFF" },
        { EquipmentCategory.MeasuringEquipment, "MEA" },
        { EquipmentCategory.ITEquipment, "IT" },
        { EquipmentCategory.HandTool, "HT" },
        { EquipmentCategory.Other, "OTH" }
    };

    /// <summary>
    /// Initializes a new instance of <see cref="AssetCodeGenerator"/>.
    /// </summary>
    /// <param name="context">The database context used to read and update sequence numbers.</param>
    public AssetCodeGenerator(FacilityDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<string> GenerateAssetCodeAsync(
        EquipmentCategory category,
        CancellationToken cancellationToken = default)
    {
        var prefix = CategoryPrefixes[category];
        var sequence = await GetNextSequenceNumberAsync(category, cancellationToken);
        return $"MAL-{prefix}-{sequence:D4}";
    }

    private async Task<int> GetNextSequenceNumberAsync(
        EquipmentCategory category,
        CancellationToken cancellationToken = default)
    {
        var sequence = await _context.AssetCodeSequences
            .FirstOrDefaultAsync(s => s.Category == category, cancellationToken);

        if (sequence == null)
        {
            sequence = new AssetCodeSequence
            {
                Category = category,
                LastSequenceNumber = 0
            };
            _context.AssetCodeSequences.Add(sequence);
        }

        sequence.LastSequenceNumber++;
        await _context.SaveChangesAsync(cancellationToken);

        return sequence.LastSequenceNumber;
    }
}
