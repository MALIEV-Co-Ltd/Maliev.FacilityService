using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Domain.Entities;

namespace Maliev.FacilityService.Application.Mapping;

/// <summary>
/// Manual mapping extension methods for converting maintenance log domain entities to DTOs.
/// </summary>
public static class MaintenanceLogMappingExtensions
{
    /// <summary>
    /// Maps an <see cref="EquipmentMaintenanceLog"/> entity to a <see cref="MaintenanceLogDto"/>.
    /// </summary>
    /// <param name="log">The maintenance log entity to map.</param>
    /// <returns>A <see cref="MaintenanceLogDto"/> populated from the entity.</returns>
    public static MaintenanceLogDto ToDto(this EquipmentMaintenanceLog log) =>
        new()
        {
            Id = log.Id,
            EquipmentId = log.EquipmentId,
            Type = log.Type,
            Description = log.Description,
            OccurredAt = log.OccurredAt,
            LoggedByEmployeeId = log.LoggedByEmployeeId,
            VendorName = log.VendorName,
            CostTHB = log.CostTHB,
            NextServiceDueDate = log.NextServiceDueDate,
            CreatedAt = log.CreatedAt,
            Documents = log.Documents
                .OrderByDescending(document => document.UploadedAt)
                .Select(document => document.ToDto())
                .ToList()
        };

    /// <summary>
    /// Maps an <see cref="EquipmentMaintenanceDocument"/> entity to a <see cref="MaintenanceLogDocumentDto"/>.
    /// </summary>
    /// <param name="document">The maintenance document entity to map.</param>
    /// <returns>A <see cref="MaintenanceLogDocumentDto"/> populated from the entity.</returns>
    public static MaintenanceLogDocumentDto ToDto(this EquipmentMaintenanceDocument document) =>
        new()
        {
            Id = document.Id,
            MaintenanceLogId = document.MaintenanceLogId,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            StoragePath = document.StoragePath,
            UploadedAt = document.UploadedAt
        };
}
