using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.UseCases.Commands.UpdateEquipment;

/// <summary>
/// Command to update an existing piece of equipment.
/// </summary>
/// <param name="EquipmentId">ID of the equipment to update. Set by the controller from the route.</param>
/// <param name="Name">New display name.</param>
/// <param name="Brand">New brand or manufacturer.</param>
/// <param name="ModelName">New model name.</param>
/// <param name="ManufacturerSerialNumber">New manufacturer serial number.</param>
/// <param name="SubCategory">New sub-category classification.</param>
/// <param name="PurchasePriceTHB">New purchase price in Thai Baht.</param>
/// <param name="PurchaseDate">New date of purchase.</param>
/// <param name="WarrantyExpiryDate">New warranty expiration date.</param>
/// <param name="NextServiceDueDate">New next scheduled service date.</param>
/// <param name="Spec">Updated category-specific spec properties.</param>
/// <param name="RowVersion">xmin row version for optimistic concurrency.</param>
public record UpdateEquipmentCommand(
    Guid EquipmentId,
    string Name,
    string? Brand,
    string? ModelName,
    string? ManufacturerSerialNumber,
    string? SubCategory,
    decimal? PurchasePriceTHB,
    DateOnly? PurchaseDate,
    DateOnly? WarrantyExpiryDate,
    DateOnly? NextServiceDueDate,
    Dictionary<string, object?>? Spec,
    uint RowVersion);
