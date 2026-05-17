namespace Maliev.FacilityService.Domain.Entities;

/// <summary>
/// Represents an uploaded document, report, finding, or image attached to an equipment maintenance log.
/// </summary>
public class EquipmentMaintenanceDocument
{
    /// <summary>
    /// Unique identifier for the maintenance document metadata record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the maintenance log.
    /// </summary>
    public Guid MaintenanceLogId { get; set; }

    /// <summary>
    /// Original file name shown to employees.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME content type of the uploaded file.
    /// </summary>
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// UploadService storage path or external file reference.
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the file metadata was attached to the maintenance log.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// Parent maintenance log.
    /// </summary>
    public EquipmentMaintenanceLog? MaintenanceLog { get; set; }
}
