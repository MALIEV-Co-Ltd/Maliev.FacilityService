namespace Maliev.FacilityService.Application.DTOs;

/// <summary>
/// DTO representing a document, finding, image, or report attached to an equipment maintenance log.
/// </summary>
public record MaintenanceLogDocumentDto
{
    /// <summary>
    /// Unique identifier of the document metadata record.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// ID of the maintenance log that owns this document.
    /// </summary>
    public Guid MaintenanceLogId { get; init; }

    /// <summary>
    /// Original file name shown to employees.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// MIME content type of the uploaded file.
    /// </summary>
    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; init; }

    /// <summary>
    /// UploadService storage path or external file reference.
    /// </summary>
    public string StoragePath { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp when the file metadata was attached to the maintenance log.
    /// </summary>
    public DateTime UploadedAt { get; init; }
}

/// <summary>
/// DTO used when creating a maintenance log with pre-uploaded document metadata.
/// </summary>
public record CreateMaintenanceLogDocumentDto
{
    /// <summary>
    /// Original file name shown to employees.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// MIME content type of the uploaded file.
    /// </summary>
    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; init; }

    /// <summary>
    /// UploadService storage path or external file reference.
    /// </summary>
    public string StoragePath { get; init; } = string.Empty;
}
